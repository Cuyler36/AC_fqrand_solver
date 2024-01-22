//#define PROFILING_ENABLED
//#define KERNEL_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms.Sequencers;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Algorithms;
using System.Diagnostics;
using ILGPU.IR.Types;

namespace AC_fqrand_solver
{
    internal class ILGPULogic
    {
        /* ILGPU Shared Kernel Params */
        const int ITERS_PER_GROUP = 8192 * 4;
        const int GROUP_SIZE = 512;
        const int GROUPS = (int)(((long)uint.MaxValue + 1) / ITERS_PER_GROUP) / GROUP_SIZE;

        /* Array containing the 'grow' type for all villagers. 0 = starter, 1 = move-in, 2 = islander */
        private static readonly int[] g_npc_grow_list =
        {
                0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1,
                1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0
            };

        /* Villager personality table */
        private static readonly int[] g_npc_looks_table =
        {
                2, 5, 0, 0, 1, 4, 1, 5, 3, 5, 5, 4, 1, 0, 0, 1,
                4, 5, 3, 4, 2, 2, 4, 1, 5, 4, 1, 3, 1, 2, 1, 4,
                2, 5, 0, 3, 4, 4, 2, 3, 0, 3, 2, 0, 3, 1, 5, 4,
                4, 5, 3, 0, 2, 0, 3, 1, 1, 5, 0, 1, 2, 3, 0, 2,
                0, 0, 2, 5, 1, 4, 3, 1, 2, 3, 2, 1, 2, 2, 3, 0,
                3, 4, 3, 1, 0, 2, 2, 4, 3, 2, 0, 3, 0, 3, 4, 2,
                5, 5, 4, 3, 4, 2, 1, 0, 2, 1, 0, 3, 5, 0, 3, 0,
                5, 1, 0, 5, 5, 0, 4, 2, 5, 1, 2, 3, 3, 3, 1, 0,
                2, 1, 4, 1, 4, 5, 4, 0, 3, 1, 5, 4, 4, 3, 5, 3,
                3, 0, 4, 2, 2, 5, 5, 3, 1, 2, 2, 4, 5, 3, 0, 3,
                4, 1, 2, 4, 0, 1, 1, 3, 3, 2, 3, 4, 5, 0, 2, 0,
                1, 5, 4, 4, 5, 1, 3, 4, 1, 4, 5, 4, 2, 5, 4, 2,
                4, 2, 3, 5, 2, 0, 5, 4, 1, 5, 0, 0, 5, 5, 1, 0,
                5, 4, 0, 1, 0, 5, 3, 1, 5, 4, 0, 3, 3, 2, 1, 4,
                2, 1, 5, 1, 0, 2, 4, 0, 1, 3, 5, 2, 2, 1
            };

        /* Generator Settings */
        const int NPC_COUNT = 0xEC;
        const int GEN_COUNT = 6;
        // seed: 0xE1090443
        unsafe static void GpuKernel(ArrayView<int> input, ArrayView<uint> output, ArrayView<int> npc_grow_list, ArrayView<int> npc_looks_table, VariableView<int> writePos, VariableView<long> beginSeed)
        {
            int globalIndex = Grid.GlobalIndex.X;
            int[] fakeTable = new int[NPC_COUNT / 4];

            // Compute our start index
            long start = beginSeed.Value + (long)globalIndex * ITERS_PER_GROUP;
            Interop.WriteLine("Start seed: {0}", start);

            /*
            if (Group.IsFirstThread)
                Interop.WriteLine("Thread #{0} starting @ {1}", globalIndex, start);
            */

            for (uint z = 0; z < ITERS_PER_GROUP; z++, start++)
            {
                for (int i = 0; i < NPC_COUNT / 4; i++)
                {
                    int pos = i << 2;
                    int write_val = (pos << 0) | ((pos + 1) << 8) | ((pos + 2) << 16) | ((pos + 3) << 24);
                    fakeTable[i] = write_val;
                }

                uint __qrand_idum = (uint)start;
                float __qrand_itemp = 0.0f;

                // We have to unroll fqrand because ILGPU doesn't allow for nested functions?
                
                for (int i = 0; i < NPC_COUNT; i++)
                {
                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    var f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot0 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot1 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    // Get int containing byte
                    int temp0 = fakeTable[slot0 / 4];
                    int temp1 = fakeTable[slot1 / 4];

                    // Calculate 'slot' index for the values
                    int shift0 = (slot0 & 3) * 8;
                    int shift1 = (slot1 & 3) * 8;

                    // Calculate necessary bit masks
                    int t0 = ~((1 << shift0) - 1);
                    int t1 = ~((1 << shift1) - 1);

                    int mask0 = ((t0 << 8) | (t0 >> 24)) & (0xFF << shift0);
                    int mask1 = ((t1 << 8) | (t1 >> 24)) & (0xFF << shift1);

                    // Get values
                    int temp3 = (temp0 >> shift0) & 0xFF;
                    int temp4 = (temp1 >> shift1) & 0xFF;

                    // Swap
                    fakeTable[slot0 / 4] = (temp0 & ~mask0) | (temp4 << shift0);
                    fakeTable[slot1 / 4] = (temp1 & ~mask1) | (temp3 << shift1);

                    //Interop.WriteLine("{0} -> {1} | {2} -> {3}", (uint)temp0, (uint)fakeTable[slot0 / 4], (uint)temp1, (uint)fakeTable[slot1 / 4]);
                }

                /* Time = ~41ms/131072 = 312 ns each */
                int personality_set_field = 0;
                int npc_idx = 0;
                int stop = 0;
                for (int i = 0; i < NPC_COUNT / 4 && stop == 0; i++)
                {
                    int t_idx = fakeTable[i];
                    for (int x = 0; x < 4; x++)
                    {
                        int idx = (t_idx >> (x * 8)) & 0xFF;
                        if (npc_grow_list[idx] == 0)
                        {
                            int personality = npc_looks_table[idx];
                            if (((personality_set_field >> personality) & 1) == 0)
                            {
                                if (idx != input[npc_idx])
                                {
                                    stop = 1;
                                    break;
                                }

                                npc_idx++;
                                if (npc_idx >= GEN_COUNT)
                                {
                                    output[writePos.Value] = (uint)start;
                                    Atomic.Add(ref writePos.Value, 1); // All villagers have been generated
                                    Interop.WriteLine("Found possible seed (GPU): {0}", (uint)start);
                                    stop = 1;
                                    break;
                                }

                                personality_set_field |= (1 << personality);
                            }
                        }
                    }
                }
            }
        }

        unsafe static void GpuKernel_InitialFilter(Index1D sIdx, ArrayView<int> input, ArrayView<uint> output, ArrayView<int> npc_grow_list, ArrayView<int> npc_looks_table, VariableView<int> writePos, VariableView<ulong> t) // , VariableView<int> loopCount
        {
            long start = (long)sIdx.X * ITERS_PER_GROUP;
            //Interop.WriteLine("Start seed: {0}", start);

            uint __qrand_idum;
            float __qrand_itemp = 0.0f;
            uint f_temp;

            int total = 0;
            ulong a = 0;

            uint[] buff = new uint[256];

            for (long z = start; z < start + ITERS_PER_GROUP; z++)
            {
                a++;
                int v0 = input[0];
                int v1 = input[1];
                int v2 = input[2];
                int v3 = input[3];
                int v4 = input[4];
                int v5 = input[5];

                __qrand_idum = (uint)z;

                /* Initial screening to determine if we should run the actual calculation for this seed.
                 * This is faster because it uses significantly less memory accesses.
                 * It works because the villagers MUST be a higher index then the previous one due to
                 * the selection algorithm. It selects the first new personality in order.
                 */
                for (int i = 0; i < NPC_COUNT; i++)
                {
                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot0 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot1 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    if (slot0 == v0)
                    {
                        v0 = slot1;
                        if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v1)
                    {
                        v1 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v2)
                    {
                        v2 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v3)
                    {
                        v3 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v4)
                    {
                        v4 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v5)
                        {
                            v5 = slot0;
                        }
                    }
                    else if (slot0 == v5)
                    {
                        v5 = slot1;
                        if (slot1 == v0)
                        {
                            v0 = slot0;
                        }
                        else if (slot1 == v1)
                        {
                            v1 = slot0;
                        }
                        else if (slot1 == v2)
                        {
                            v2 = slot0;
                        }
                        else if (slot1 == v3)
                        {
                            v3 = slot0;
                        }
                        else if (slot1 == v4)
                        {
                            v4 = slot0;
                        }
                    }
                    else if (slot1 == v0)
                    {
                        v0 = slot0;
                        if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v1)
                    {
                        v1 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v2)
                    {
                        v2 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v3)
                    {
                        v3 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v4)
                    {
                        v4 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v5)
                        {
                            v5 = slot1;
                        }
                    }
                    else if (slot1 == v5)
                    {
                        v5 = slot0;
                        if (slot0 == v0)
                        {
                            v0 = slot1;
                        }
                        else if (slot0 == v1)
                        {
                            v1 = slot1;
                        }
                        else if (slot0 == v2)
                        {
                            v2 = slot1;
                        }
                        else if (slot0 == v3)
                        {
                            v3 = slot1;
                        }
                        else if (slot0 == v4)
                        {
                            v4 = slot1;
                        }
                    }
                }

                // We check here if the order is wrong, faster? than doing the rest.
                if (v0 < v1 && v1 < v2 && v2 < v3 && v3 < v4 && v4 < v5)
                {
                    //Interop.WriteLine("Found possible seed (GPU): {0}", (uint)z);
                    byte[] fakeTable = new byte[NPC_COUNT];
                    // Run actual calc
                    for (byte i = 0; i < NPC_COUNT; i++)
                    {
                        /*
                        int pos = i << 2;
                        int write_val = (pos << 0) | ((pos + 1) << 8) | ((pos + 2) << 16) | ((pos + 3) << 24);
                        fakeTable[i] = write_val;
                        */
                        fakeTable[i] = i;
                    }

                    __qrand_idum = (uint)z;

                    // We have to unroll fqrand because ILGPU doesn't allow for nested functions?

                    for (int i = 0; i < NPC_COUNT; i++)
                    {
                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                        f_temp = (__qrand_idum >> 9) | 0x3F800000;
                        __qrand_itemp = *(float*)&f_temp;

                        int slot0 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                        f_temp = (__qrand_idum >> 9) | 0x3F800000;
                        __qrand_itemp = *(float*)&f_temp;

                        int slot1 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                        // Get int containing byte
                        //int temp0 = fakeTable[slot0 / 4];
                        //int temp1 = fakeTable[slot1 / 4];

                        // Calculate 'slot' index for the values
                        //int shift0 = (slot0 & 3) * 8;
                        //int shift1 = (slot1 & 3) * 8;

                        // Calculate necessary bit masks
                        //int t0 = ~((1 << shift0) - 1);
                        //int t1 = ~((1 << shift1) - 1);

                        //int mask0 = ((t0 << 8) | (t0 >> 24)) & (0xFF << shift0);
                        //int mask1 = ((t1 << 8) | (t1 >> 24)) & (0xFF << shift1);

                        // Get values
                        //int temp3 = (temp0 >> shift0) & 0xFF;
                        //int temp4 = (temp1 >> shift1) & 0xFF;

                        // Swap
                        //fakeTable[slot0 / 4] = (temp0 & ~mask0) | (temp4 << shift0);
                        //fakeTable[slot1 / 4] = (temp1 & ~mask1) | (temp3 << shift1);
                        byte t0 = fakeTable[slot0];
                        fakeTable[slot0] = fakeTable[slot1];
                        fakeTable[slot1] = t0;

                        //Interop.WriteLine("{0} -> {1} | {2} -> {3}", (uint)temp0, (uint)fakeTable[slot0 / 4], (uint)temp1, (uint)fakeTable[slot1 / 4]);
                    }

                    /* Time = ~41ms/131072 = 312 ns each */
                    int personality_set_field = 0;
                    int npc_idx = 0;
                    int stop = 0;
                    for (int i = 0; i < NPC_COUNT && stop == 0; i++)
                    {
                        int t_idx = fakeTable[i];
                        //for (int x = 0; x < 4; x++)
                        {
                            //int idx = (t_idx >> (x * 8)) & 0xFF;
                            int idx = t_idx;
                            if (npc_grow_list[idx] == 0)
                            {
                                int personality = npc_looks_table[idx];
                                if (((personality_set_field >> personality) & 1) == 0)
                                {
                                    if (idx != input[npc_idx])
                                    {
                                        stop = 1;
                                        break;
                                    }

                                    npc_idx++;
                                    if (npc_idx >= GEN_COUNT)
                                    {
                                        //output[writePos.Value] = (uint)start;
                                        buff[total] = (uint)z;
                                        total++;
                                        //Atomic.Add(ref writePos.Value, 1); // All villagers have been generated
                                        //Interop.WriteLine("Found possible seed (GPU): {0}", (uint)start);
                                        stop = 1;
                                        break;
                                    }

                                    personality_set_field |= (1 << personality);
                                }
                            }
                        }
                    }
                    //output[writePos.Value] = (uint)start;
                }
            }

            for (int i = 0; i < total; i++)
            {
                output[writePos.Value + i] = buff[i];
            }
            Atomic.Add(ref writePos.Value, total);
            Atomic.Add(ref t.Value, a);
        }

        unsafe static void GpuKernel64Bit(ArrayView<int> input, ArrayView<uint> output, ArrayView<int> npc_grow_list, ArrayView<int> npc_looks_table, VariableView<int> writePos, VariableView<long> beginSeed)
        {
            int globalIndex = Grid.GlobalIndex.X;
            long[] fakeTable = new long[(NPC_COUNT + 7) / 8];

            // Compute our start index
            long start = beginSeed.Value + (long)globalIndex * ITERS_PER_GROUP;
            
            /*
            if (Group.IsFirstThread)
                Interop.WriteLine("Thread #{0} starting @ {1}", globalIndex, start);
            */


            for (uint z = 0; z < ITERS_PER_GROUP; z++, start++)
            {
                for (int i = 0; i < NPC_COUNT / 8; i++)
                {
                    long pos = i << 3;
                    long write_val = (pos << 0) | ((pos + 1) << 8) | ((pos + 2) << 16) | ((pos + 3) << 24) | ((pos + 4) << 32) | ((pos + 5) << 40) | ((pos + 6) << 48) | ((pos + 7) << 56);
                    fakeTable[i] = write_val;
                }

                uint __qrand_idum = (uint)start;
                float __qrand_itemp = 0.0f;

                // We have to unroll fqrand because ILGPU doesn't allow for nested functions?

                for (int i = 0; i < NPC_COUNT; i++)
                {
                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    var f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot0 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    f_temp = (__qrand_idum >> 9) | 0x3F800000;
                    __qrand_itemp = *(float*)&f_temp;

                    int slot1 = (int)(NPC_COUNT * (__qrand_itemp - 1.0f));

                    // Get int containing byte
                    long temp0 = fakeTable[slot0 / 8];
                    long temp1 = fakeTable[slot1 / 8];

                    // Calculate 'slot' index for the values
                    int shift0 = (slot0 & 7) * 8;
                    int shift1 = (slot1 & 7) * 8;

                    // Calculate necessary bit masks
                    long t0 = ~((1L << shift0) - 1);
                    long t1 = ~((1L << shift1) - 1);

                    long mask0 = ((t0 << 8) | (t0 >> 56)) & (0xFFL << shift0);
                    long mask1 = ((t1 << 8) | (t1 >> 56)) & (0xFFL << shift1);

                    // Get values
                    long temp3 = (temp0 >> shift0) & 0xFF;
                    long temp4 = (temp1 >> shift1) & 0xFF;

                    // Swap
                    fakeTable[slot0 / 8] = (temp0 & ~mask0) | (temp4 << shift0);
                    fakeTable[slot1 / 8] = (temp1 & ~mask1) | (temp3 << shift1);

                    //Interop.WriteLine("{0} -> {1} | {2} -> {3}", (uint)temp0, (uint)fakeTable[slot0 / 8], (uint)temp1, (uint)fakeTable[slot1 / 8]);
                }

                /* Time = ~41ms/131072 = 312 ns each */
                int personality_set_field = 0;
                int npc_idx = 0;
                int stop = 0;
                for (int i = 0; i < NPC_COUNT / 8 && stop == 0; i++)
                {
                    long t_idx = fakeTable[i];
                    for (int x = 0; x < 8; x++)
                    {
                        int idx = (int)(t_idx >> (x * 8)) & 0xFF;
                        if (npc_grow_list[idx] == 0)
                        {
                            int personality = npc_looks_table[idx];
                            if (((personality_set_field >> personality) & 1) == 0)
                            {
                                if (idx != input[npc_idx])
                                {
                                    stop = 1;
                                    break;
                                }

                                npc_idx++;
                                if (npc_idx >= GEN_COUNT)
                                {
                                    output[writePos.Value] = (uint)start;
                                    Atomic.Add(ref writePos.Value, 1); // All villagers have been generated
                                    Interop.WriteLine("Found possible seed (GPU): {0}", (uint)start);
                                    stop = 1;
                                    break;
                                }

                                personality_set_field |= (1 << personality);
                            }
                        }
                    }
                }
            }
        }

        public static async Task<List<uint>> Execute(int[] villagers, long beginPos = 0, long count = (long)uint.MaxValue + 1, string gpuAdapterName = null)
        {
#if PROFILING_ENABLED
            using Context ilgpu_context = Context.Create(builder => builder.Default().Profiling());
#else
            using Context ilgpu_context = Context.CreateDefault();
#endif

            for (int i = 0; i < villagers.Length; i++)
            {
                Console.WriteLine($"{villagers[i]:X2} - {g_npc_looks_table[villagers[i]]} | {g_npc_grow_list[villagers[i]]}");
            }

            foreach (var device in ilgpu_context)
            {
                Console.WriteLine(device.Name);
#if KERNEL_DEBUG
                if (device.Name != "CPUAccelerator") continue;
#else
                if (device.Name == "CPUAccelerator") continue;
                if (gpuAdapterName != null && device.Name != gpuAdapterName) continue;
#endif
                using Accelerator accelerator = device.CreateAccelerator(ilgpu_context);
                using AcceleratorStream stream = accelerator.CreateStream();

                //new int[] { 0x94, 0x9B, 0x04, 0x22, 0x97, 0x18 }
                var inputData = accelerator.Allocate1D(villagers);
                
                var output = accelerator.Allocate1D<uint>(1000);
                output.MemSetToZero();

                var writePos = accelerator.Allocate1D<int>(1);
                writePos.MemSetToZero();

                var begin = accelerator.Allocate1D(new long[] { beginPos });

                int groupSize = accelerator.MaxNumThreadsPerGroup;
                int gridSize = accelerator.MaxNumThreadsPerGroup;
                int loopCount = (int)Math.Ceiling((float)count / (float)(groupSize * gridSize));
                //int groups = (int)(count / ITERS_PER_GROUP) / groupSize;

                Console.WriteLine($"GroupSize: {groupSize} | GridSize: {gridSize} | CountPerThread: {loopCount}");

                //var lCount = accelerator.Allocate1D<int>(new int[] { loopCount });

#if KERNEL_DEBUG
                KernelConfig dimension = new(groups, groupSize);
#else
                KernelConfig dimension = new(gridSize, groupSize);
#endif

#if PROFILING_ENABLED
                using var startMarker = stream.AddProfilingMarker();
#endif

                var t = accelerator.Allocate1D(new ulong[] { 0 });
                var v_npc_grow_list = accelerator.Allocate1D(g_npc_grow_list);
                var v_npc_looks_table = accelerator.Allocate1D(g_npc_looks_table);

#if TEST
                var kernel = accelerator.LoadKernel<ArrayView<int>, ArrayView<uint>, ArrayView<int>, ArrayView<int>, VariableView<int>, VariableView<long>>(GpuKernel);

                var sw = Stopwatch.StartNew();
                kernel(stream, dimension, inputData.View, output.View, v_npc_grow_list.View, v_npc_looks_table.View, writePos.View.VariableView(0), begin.View.VariableView(0));
#else

                var sw = Stopwatch.StartNew();
                var kernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<uint>, ArrayView<int>, ArrayView<int>, VariableView<int>, VariableView<ulong>>(GpuKernel_InitialFilter); // , VariableView<int>
                kernel(stream, (int)((((ulong)uint.MaxValue) + 1) / ITERS_PER_GROUP), inputData.View, output.View, v_npc_grow_list.View, v_npc_looks_table.View, writePos.View.VariableView(0), t.View.VariableView(0)); // lCount.View.VariableView(0)
#endif

                await Task.Run(() => accelerator.Synchronize());
                sw.Stop();
                ulong[] a = new ulong[1];
                t.CopyToCPU(a);
                Console.WriteLine($"{a[0]:X}");
                Console.WriteLine($"GPU finished! Took {(int)(sw.ElapsedTicks / Stopwatch.Frequency)} seconds.");
#if PROFILING_ENABLED
                using var endMarker = stream.AddProfilingMarker();
                Console.WriteLine($"Elapsed time: {(int)(endMarker - startMarker).TotalMilliseconds}ms");
#endif

                uint[] CPU_BUF = new uint[1000];
                output.CopyToCPU(CPU_BUF);

                int[] WRITE_POS = new int[1];
                writePos.CopyToCPU(WRITE_POS);
                Console.WriteLine($"WritePos: {WRITE_POS[0]}");

                List<uint> results = new();
                for (int i = 0; i < WRITE_POS[0]; i++)
                {
                    Console.WriteLine($"GPU Result: {CPU_BUF[i]:X8}");
                    results.Add(CPU_BUF[i]);
                }

                return results;
            }

            return new List<uint>();
        }

        private static void SeedSearchKernel(Index1D i, VariableView<uint> target, VariableView<int> minRolls, VariableView<int> maxRolls, ArrayView<uint> resultPool, VariableView<int> resultIdx)
        {
            // Compute our start index
            //Interop.Write("{0}\n", (uint)i.X);
            //uint start = (uint)Grid.GlobalIndex.X * ITERS_PER_GROUP;
            uint start = (uint)i.X * ITERS_PER_GROUP;
            uint __qrand_idum;

            uint m_target = target.Value;
            int m_minRolls = minRolls.Value;
            int m_maxRolls = maxRolls.Value;

            uint idx;
            for (idx = 0; idx < ITERS_PER_GROUP; idx++)
            {
                __qrand_idum = start + idx;         

                for (int _ = 0; _ < m_minRolls; _++)
                {
                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                }

                for (int _ = m_minRolls; _ < m_maxRolls; _++)
                {
                    if (__qrand_idum == m_target)
                    {
                        resultPool[resultIdx.Value] = start + idx;
                        Atomic.Add(ref resultIdx.Value, 1);
                        //Interop.Write("{0} -> {1}\n", (uint)Grid.GlobalIndex.X, start + idx);
                        break;
                    }

                    __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                }
            }

            //Atomic.Add(ref totalIters.Value, idx);
        }

        public static async Task<List<uint>> ExecuteSeedSearchKernel(uint targetSeed, int minRolls = 83, int maxRolls = 5000, string gpuAdapterName = null)
        {
            using Context ilgpu_context = Context.CreateDefault();

            foreach (var device in ilgpu_context)
            {
                Console.WriteLine(device.Name);
#if DEBUG && KERNEL_DEBUG
                if (device.Name != "CPUAccelerator") continue;
#else
                if (device.Name == "CPUAccelerator") continue;
                if (gpuAdapterName != null && device.Name != gpuAdapterName) continue;
#endif
                using Accelerator accelerator = device.CreateAccelerator(ilgpu_context);
                using AcceleratorStream stream = accelerator.CreateStream();

                var targetView = accelerator.Allocate1D(new uint[] { targetSeed });
                var paramView = accelerator.Allocate1D(new int[] { minRolls, maxRolls });
                var resultSeeds = accelerator.Allocate1D<uint>(10_000);
                var resultsIdx = accelerator.Allocate1D<int>(1);

                resultSeeds.MemSetToZero();
                resultsIdx.MemSetToZero();

                var kernel = accelerator.LoadAutoGroupedKernel<Index1D, VariableView<uint>, VariableView<int>, VariableView<int>, ArrayView<uint>, VariableView<int>>(SeedSearchKernel);
                kernel(stream, (int)(((long)uint.MaxValue + 1) / ITERS_PER_GROUP), targetView.View.VariableView(0), paramView.View.VariableView(0), paramView.View.VariableView(1), resultSeeds.View, resultsIdx.View.VariableView(0));

                var sw = Stopwatch.StartNew();
                await Task.Run(() => accelerator.Synchronize());
                sw.Stop();
                Console.WriteLine($"GPU finished! Took {(int)(sw.ElapsedTicks / Stopwatch.Frequency)} seconds.");

                uint[] CPU_BUF = new uint[10_000];
                resultSeeds.CopyToCPU(CPU_BUF);

                int[] WRITE_POS = new int[1];
                resultsIdx.CopyToCPU(WRITE_POS);
                Console.WriteLine($"WritePos: {WRITE_POS[0]}");

                List<uint> results = new();
                for (int i = 0; i < WRITE_POS[0]; i++)
                {
                    //Console.WriteLine($"GPU Result: {CPU_BUF[i]:X8}");
                    results.Add(CPU_BUF[i]);
                }

                return results;
            }

            return new List<uint>();
        }

        private static unsafe void TownSeedSearchKernel(Index1D i, VariableView<uint> target, VariableView<int> fruit, VariableView<int> grass, ArrayView<uint> resultPool, VariableView<int> resultIdx)
        {
            // Compute our start index
            //Interop.Write("{0}\n", (uint)i.X);
            //uint start = (uint)Grid.GlobalIndex.X * ITERS_PER_GROUP;
            //Atomic.Add(ref runCount.Value, 1UL);
            uint start = (uint)i.X * ITERS_PER_GROUP;
            uint __qrand_idum;
            int m_fruit = fruit.Value;
            int m_grass = grass.Value;

            uint[] buf = new uint[1024];
            int count = 0;
            uint m_target = target.Value;
            uint idx;
            for (idx = 0; idx < ITERS_PER_GROUP; idx++)
            {
                __qrand_idum = start + idx;
                uint temp;
                float f;

                // fruit
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;

                if ((int)((f - 1.0f) * 5) != m_fruit) continue;

                // grass
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;

                if ((int)((f - 1.0f) * 3) != m_grass) continue;

                // Step Mode
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;
                
                int stepMode = (int)((f - 1.0f) * 100);

                if (stepMode >= 15)
                {
                    for (int _ = 1; _ < 27; _++)
                    {
                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    }

                    for (int _ = 27; _ < 5000; _++)
                    {
                        if (__qrand_idum == m_target)
                        {
                            //resultPool[resultIdx.Value] = start + idx;
                            //Atomic.Add(ref resultIdx.Value, 1);
                            //Interop.Write("{0} -> {1}\n", (uint)Grid.GlobalIndex.X, start + idx);
                            buf[count++] = start + idx;
                            break;
                        }

                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    }
                }
            }

            for (int x = 0; x < count; x++)
            {
                resultPool[resultIdx.Value + x] = buf[x];
            }
            Atomic.Add(ref resultIdx.Value, count);
            //Atomic.Add(ref totalIters.Value, idx);
        }

        private static unsafe void TownSeedSearchKernel3Layered(Index1D i, VariableView<uint> target, VariableView<int> fruit, VariableView<int> grass, ArrayView<uint> resultPool, VariableView<int> resultIdx)
        {
            // Compute our start index
            //Interop.Write("{0}\n", (uint)i.X);
            //uint start = (uint)Grid.GlobalIndex.X * ITERS_PER_GROUP;
            //Atomic.Add(ref runCount.Value, 1UL);
            uint start = (uint)i.X * ITERS_PER_GROUP;
            uint __qrand_idum;
            int m_fruit = fruit.Value;
            int m_grass = grass.Value;

            uint[] buf = new uint[1024];
            int count = 0;
            uint m_target = target.Value;
            uint idx;
            for (idx = 0; idx < ITERS_PER_GROUP; idx++)
            {
                __qrand_idum = start + idx;
                uint temp;
                float f;

                // fruit
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;

                if ((int)((f - 1.0f) * 5) != m_fruit) continue;

                // grass
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;

                if ((int)((f - 1.0f) * 3) != m_grass) continue;

                // Step Mode
                __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                temp = (__qrand_idum >> 9) | 0x3F800000;
                f = *(float*)&temp;

                int stepMode = (int)((f - 1.0f) * 100);

                if (stepMode < 15)
                {
                    for (int _ = 1; _ < 27; _++)
                    {
                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    }

                    for (int _ = 27; _ < 5000; _++)
                    {
                        if (__qrand_idum == m_target)
                        {
                            //resultPool[resultIdx.Value] = start + idx;
                            //Atomic.Add(ref resultIdx.Value, 1);
                            //Interop.Write("{0} -> {1}\n", (uint)Grid.GlobalIndex.X, start + idx);
                            buf[count++] = start + idx;
                            break;
                        }

                        __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
                    }
                }
            }

            for (int x = 0; x < count; x++)
            {
                resultPool[resultIdx.Value + x] = buf[x];
            }
            Atomic.Add(ref resultIdx.Value, count);
            //Atomic.Add(ref totalIters.Value, idx);
        }

        public static async Task<List<uint>> ExecuteTownSeedSearch(uint targetSeed, int fruit, int grass, bool threeLayered = false, long beginPos = 0, long count = (long)uint.MaxValue + 1, string gpuAdapterName = null)
        {
            using Context ilgpu_context = Context.CreateDefault();

            foreach (var device in ilgpu_context)
            {
                Console.WriteLine(device.Name);
#if DEBUG && KERNEL_DEBUG
                if (device.Name != "CPUAccelerator") continue;
#else
                if (device.Name == "CPUAccelerator" || device.Name == "gfx1036") continue;
                if (gpuAdapterName != null && device.Name != gpuAdapterName) continue;
#endif
                using Accelerator accelerator = device.CreateAccelerator(ilgpu_context);
                using AcceleratorStream stream = accelerator.CreateStream();

                var targetView = accelerator.Allocate1D(new uint[] { targetSeed });
                var fruitView = accelerator.Allocate1D(new int[] { fruit });
                var grassView = accelerator.Allocate1D(new int[] { grass });
                var resultSeeds = accelerator.Allocate1D<uint>(10_000);
                var resultsIdx = accelerator.Allocate1D<int>(1);
                //var runCount = accelerator.Allocate1D<ulong>(1);

                resultSeeds.MemSetToZero();
                resultsIdx.MemSetToZero();
                //runCount.MemSetToZero();
                Action<Index1D, VariableView<uint>, VariableView<int>, VariableView<int>, ArrayView<uint>, VariableView<int>> kernelFunc = threeLayered ? TownSeedSearchKernel3Layered : TownSeedSearchKernel;
                var kernel = accelerator.LoadAutoGroupedKernel(kernelFunc);
                kernel(stream, (int)(((long)uint.MaxValue + 1) / ITERS_PER_GROUP), targetView.View.VariableView(0), fruitView.View.VariableView(0), grassView.View.VariableView(0), resultSeeds.View, resultsIdx.View.VariableView(0));

                var sw = Stopwatch.StartNew();
                await Task.Run(() => accelerator.Synchronize());
                sw.Stop();
                Console.WriteLine($"GPU finished! Took {(int)(sw.ElapsedTicks / Stopwatch.Frequency)} seconds.");

                uint[] CPU_BUF = new uint[10_000];
                resultSeeds.CopyToCPU(CPU_BUF);

                int[] WRITE_POS = new int[1];
                resultsIdx.CopyToCPU(WRITE_POS);
                Console.WriteLine($"WritePos: {WRITE_POS[0]}");
                //ulong[] RUN_COUNT = new ulong[1];
                //runCount.CopyToCPU(RUN_COUNT);
                //Console.WriteLine($"Run Count: {RUN_COUNT[0]:X}");

                List<uint> results = new();
                for (int i = 0; i < WRITE_POS[0]; i++)
                {
                    //Console.WriteLine($"GPU Result: {CPU_BUF[i]:X8}");
                    results.Add(CPU_BUF[i]);
                }

                return results;
            }

            return new List<uint>();
        }
    }
}
