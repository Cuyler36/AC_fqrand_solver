using ILGPU;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AC_fqrand_solver
{
    public partial class AllDebtsForm : Form
    {
        private readonly string[] days =
        {
            "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
        };

        private readonly ComboBox[] villagerBoxes;
        private readonly InterpolatedPictureBox[] trainStationBoxes;

        private int selectedTrainStationIdx = -1;
        private List<uint> lastGeneratedInitialSeeds = null;
        private List<uint> seeds = null;
        private List<InitialResult> results = null;
        private List<uint> possible_seeds = null;
        private bool mTownStateChanged = true;
        private uint mLastTownSeed = 0;

        public AllDebtsForm()
        {
            InitializeComponent();

            /* Initialize villager combo boxes */
            villagerBoxes = new ComboBox[]
            {
                VillagerBox1, VillagerBox2, VillagerBox3, VillagerBox4, VillagerBox5, VillagerBox6
            };

            foreach (ComboBox villagerBox in villagerBoxes)
            {
                villagerBox.DataSource = new BindingSource(InitialTownLogic.StartingVillagerDict, null);
                villagerBox.DisplayMember = "Value";
                villagerBox.ValueMember = "Key";
            }

            /* Initialize Furniture combo box */
            FurnitureBox.DataSource = new BindingSource(InitialTownLogic.NookItems, null);
            FurnitureBox.DisplayMember = "Value";
            FurnitureBox.ValueMember = "Key";

            UmbrellaBox.SelectedIndex = 0;

            /* Initialize Train Station Picture Boxes */
            trainStationBoxes = new InterpolatedPictureBox[15];

            for (int i = 0; i < 15; i++)
            {
                trainStationBoxes[i] = new InterpolatedPictureBox
                {
                    InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor,
                    Image = TrainStationImageList.Images[i],
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(32, 32),
                    Cursor = Cursors.Hand
                };

                trainStationBoxes[i].MouseEnter += OnTrainStationPictureBoxEnter;
                trainStationBoxes[i].MouseLeave += OnTrainStationPictureBoxLeave;
                trainStationBoxes[i].Click += OnTrainStationClick;
                TrainStationLayout.Controls.Add(trainStationBoxes[i]);
            }

            CPUThreadCntBox.Maximum = Environment.ProcessorCount;
            CPUThreadCntBox.Value = Math.Max(1, CPUThreadCntBox.Maximum - 2);

            RefineSeedsButton.Enabled = false;
            GetResultsButton.Enabled = false;

            fruitComboBox.SelectedIndex = 0;
            grassTypeBox.SelectedIndex = 0;

            /* Initialize GPU adapter options */
            using Context ilgpu_context = Context.CreateDefault();
            foreach (var device in ilgpu_context)
            {
                if (device.AcceleratorType != ILGPU.Runtime.AcceleratorType.CPU)
                {
                    gpuAdapterBox.Items.Add(device.Name);
                    if (gpuAdapterBox.SelectedIndex == -1)
                    {
                        gpuAdapterBox.SelectedIndex = 0;
                    }
                }

                if (device.AcceleratorType == ILGPU.Runtime.AcceleratorType.Cuda)
                {
                    gpuAdapterBox.SelectedIndex = gpuAdapterBox.Items.Count - 1;
                }
            }

            // TEST
            //InitialTownLogic.RefineInitialSeeds(new List<uint>() { 0xE1090443 }, 12);
            //InitialTownLogic.DEBUG_FindFQRandCallsFromStartSeedToNextSeed(0xD63CD25B, 0x1296F751);
            //InitialTownLogic.FindTurnipPriceGenSeed(new List<uint>() { 0x1296F751 }, 0, 1, 0x3240, 0x2220);
            //InitialTownLogic.DEBUG_FindFQRandSeedsWhichResultInSeed(0xA2E32036);

            //TownGeneration.DEBUG_PrintAllDataCombiTypeInfo();
            //return;
            //return;

            /*
            ILGPULogic.ExecuteTownSeedSearch(0xA2E32036);
            return;

            TownGeneration.DEBUG_TestSeedSearchTEST();
            return;
            */

            //ILGPULogic.ExecuteTownSeedSearch(0xE567D371, 2, 2);
            //return;

            var gen = new TownGeneration(0x3BF976B3);
            /*
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1_000_000; i++)
            {
                //gen.GenerateBGOptimal();
                //var fg = gen.GenerateFG(bg, 3);
            }
            sw.Stop();
            *
            Console.WriteLine($"Average calls: {(double)gen.total_fqrand_calls / 1_000_000}");
            Console.WriteLine($"Min calls: {gen.min_gen_fqrand_calls}");
            Console.WriteLine($"Max calls: {gen.max_gen_fqrand_calls}");

            using TextWriter writer = File.CreateText(@"C:\Users\olsen\Desktop\fqrand_calls.txt");
            var ordered = gen.calls_dict.OrderBy((kv) => kv.Key);
            foreach (var kv in ordered)
            {
                writer.WriteLine($"{kv.Key}\t{kv.Value}");
            }

            Console.WriteLine($"Took: {((double)sw.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond) / 1_000_000d} ms");
            */

            ushort[] bg = gen.GenerateBG();
            ushort[][] fg = gen.GenerateFG(bg, 3);

            int[] acres = new int[30];
            List<int[][]> data = new();

            uint last_idum = 0xB7548812;
            gen.__qrand_idum = last_idum;

            for (int i = 0; i < 300; i++)
            {
                gen.fqrand_raw();
            }

            for (int i = 0; i < 250; i++)
            {
                gen.__qrand_idum = last_idum;
                for (int x = 0; x < i; x++)
                {
                    gen.fqrand_raw();
                }
                last_idum = gen.__qrand_idum;
                gen.__qrand_idum = last_idum;
                gen.GetShineSpotAndMoneyStonePositions(bg, fg, out int[][] locs);
                data.Add(locs);
                acres[(locs[0][0] - 1) + (locs[0][1] - 1) * 5]++;
            }

            for (int z = 0; z < 6; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Console.WriteLine($"{(char)('A' + z)}-{x + 1}\t{acres[x + z * 5]}");
                }
            }

            new MapProbabilityForm(bg, gen.GetMapIcons(bg), data).Show();

            return;
            
            
        }

        private void OnTrainStationPictureBoxEnter(object sender, EventArgs e)
        {
            InterpolatedPictureBox pb = sender as InterpolatedPictureBox;

            pb.BackColor = Color.Gray;
        }

        private void OnTrainStationPictureBoxLeave(object sender, EventArgs e)
        {
            InterpolatedPictureBox pb = sender as InterpolatedPictureBox;

            if (selectedTrainStationIdx != -1 && Array.IndexOf(trainStationBoxes, pb) == selectedTrainStationIdx)
            {
                pb.BackColor = SystemColors.Highlight;
            }
            else
            {
                pb.BackColor = SystemColors.Control;
            }
        }

        private void OnTrainStationClick(object sender, EventArgs e)
        {
            InterpolatedPictureBox pb = sender as InterpolatedPictureBox;
            int idx = Array.IndexOf(trainStationBoxes, pb);
            if (idx != -1 && idx != selectedTrainStationIdx)
            {
                if (selectedTrainStationIdx != -1)
                {
                    trainStationBoxes[selectedTrainStationIdx].BackColor = SystemColors.Control;
                }

                selectedTrainStationIdx = idx;
                pb.BackColor = SystemColors.Highlight;
            }
        }

        private void UpdateSeedsBox()
        {
            SeedListBox.Items.Clear();
            if (seeds != null)
            {
                foreach (uint seed in seeds)
                {
                    SeedListBox.Items.Add(seed.ToString("X8"));
                }
            }
        }

        private async void FindSeedsButton_Click(object sender, EventArgs e)
        {
            /* Get villager settings */
            int[] villagers = new int[6];
            for (int i = 0; i < 6; i++)
            {
                if (villagerBoxes[i].SelectedValue == null)
                {
                    /* Try looking up the index from the text in case SelectedValue wasn't set. */
                    string s = villagerBoxes[i].Text.ToLower();
                    var results = InitialTownLogic.StartingVillagerDict.Where(o => o.Value.ToLower() == s);
                    if (results.Count() == 0)
                    {
                        MessageBox.Show($"Please selected a valid villager for villager #{i + 1}!", "Invalid Villager Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        villagers[i] = results.ElementAt(0).Key;
                    }
                }
                else
                {
                    villagers[i] = (byte)villagerBoxes[i].SelectedValue;
                }

                if (Array.IndexOf(villagers, villagers[i]) < i)
                {
                    MessageBox.Show($"Duplicate villager #{i + 1} detected!", "Duplicate Villager Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            double percent = (double)CPUNumericBox.Value;
            if (percent < 0) percent = 0;
            else if (percent > 100) percent = 100;

            FindSeedsButton.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Enabled = true;
            seeds = await InitialTownLogic.SeedBruteforce_ComboCPUGPU(villagers, percent / 100.0d, (int)CPUThreadCntBox.Value, gpuAdapterBox.SelectedIndex == -1 ? null : (string)gpuAdapterBox.Items[gpuAdapterBox.SelectedIndex]);
            lastGeneratedInitialSeeds = seeds;
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = 100;
            UpdateSeedsBox();

            FindSeedsButton.Enabled = true;
            RefineSeedsButton.Enabled = true;
            GetResultsButton.Enabled = true;
        }

        private void RefineSeedsButton_Click(object sender, EventArgs e)
        {
            if (seeds != null)
            {
                if (selectedTrainStationIdx == -1)
                {
                    MessageBox.Show($"Train Station type has not been set!", "Train Station Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                RefineSeedsButton.Enabled = false;
                seeds = InitialTownLogic.RefineInitialSeeds(seeds, selectedTrainStationIdx);
                UpdateSeedsBox();
            }
        }

        private void GetResultsButton_Click(object sender, EventArgs e)
        {
            ResultsListBox.Items.Clear();

            if (seeds != null)
            {
                ushort ftr, umbrella;
                if (FurnitureBox.SelectedValue != null)
                {
                    ftr = (ushort)FurnitureBox.SelectedValue;
                }
                else
                {
                    string s = FurnitureBox.Text.ToLower();
                    var res = InitialTownLogic.NookItems.Where(o => o.Value.ToLower() == s);
                    if (res.Count() == 0)
                    {
                        MessageBox.Show($"Please select a valid Nook Furniture!", "Furniture Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ftr = (ushort)res.ElementAt(0).Key;
                }

                if (UmbrellaBox.SelectedIndex != -1)
                {
                    umbrella = (ushort)(0x2204u + UmbrellaBox.SelectedIndex);
                }
                else
                {
                    var res = UmbrellaBox.Items.IndexOf(UmbrellaBox.Text.ToLower());
                    if (res == -1)
                    {
                        MessageBox.Show($"Please select a valid Nook Umbrella!", "Umbrella Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    umbrella = (ushort)(0x2204u + res);
                }

                results = InitialTownLogic.FindTurnipPriceGenSeed(seeds, (int)MinFQRandBox.Value, (int)MaxFQRandBox.Value, ftr, umbrella);

                foreach (InitialResult result in results)
                {
                    ResultsListBox.Items.Add($"{result.InitialSeed:X8}-{result.TrainSeed:X8}\n\tTrend: {result.StalkMarketTrend}\n\tSunday Price: {result.SundayPrice}\n\tSpike Day: {days[result.SpikeDay]}");
                }
            }
            else
            {
                MessageBox.Show($"You must generate (and refine if necessary) seeds first!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreSeedsButton_Click(object sender, EventArgs e)
        {
            if (lastGeneratedInitialSeeds != null)
            {
                seeds = lastGeneratedInitialSeeds;
                RefineSeedsButton.Enabled = true;
                UpdateSeedsBox();
            }
        }

        private static readonly IReadOnlyDictionary<string, int> acreIndexLUT = new Dictionary<string, int>()
        {
            { "B-1", 15 },
            { "B-2", 16 },
            { "B-4", 18 },
            { "B-5", 19 },
            { "C-1", 22 },
            { "C-2", 23 },
            { "C-3", 24 },
            { "C-4", 25 },
            { "C-5", 26 },
            { "D-1", 29 },
            { "D-2", 30 },
            { "D-3", 31 },
            { "D-4", 32 },
            { "D-5", 33 },
            { "E-1", 36 },
            { "E-2", 37 },
            { "E-3", 38 },
            { "E-4", 39 },
            { "E-5", 40 }
        };

        private async void TownButton_Click(object sender, EventArgs e)
        {
            if (LakeAcreListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select the Lake Acre!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (WishingWellListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select the Wishing Well Acre!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (PoliceStationListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select the Police Station Acre!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MuseumListBox.SelectedItem == null)
            {
                MessageBox.Show("You must select the Museum Acre!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int lake_acre = acreIndexLUT[(string)LakeAcreListBox.SelectedItem];
            int well_acre = acreIndexLUT[(string)WishingWellListBox.SelectedItem];
            int police_acre = acreIndexLUT[(string)PoliceStationListBox.SelectedItem];
            int museum_acre = acreIndexLUT[(string)MuseumListBox.SelectedItem];

            Console.WriteLine($"{well_acre} {lake_acre} {police_acre} {museum_acre}");

            if (ResultsListBox.Items.Count < 1)
            {
                MessageBox.Show("You cannot search for your town without finding possible seeds first!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (ResultsListBox.Items.Count > 1 && ResultsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Since multiple seeds were found, you must select one to search for your town!", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            InitialResult result = ResultsListBox.Items.Count == 1 ? results[0] : results[ResultsListBox.SelectedIndex];
            uint seedToSearchFor = result.InitialSeed;
            Console.WriteLine($"{seedToSearchFor:X8}");

            TownButton.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;

            //await InitialTownLogic.DEBUG_FindFQRandCallsFromStartSeedToNextSeed(0x05a3e56e, 0xc7621027);
            if (seedToSearchFor != mLastTownSeed || mTownStateChanged)
            {
                mTownStateChanged = false;
                mLastTownSeed = seedToSearchFor;
                possible_seeds = await ILGPULogic.ExecuteTownSeedSearch(seedToSearchFor, fruitComboBox.SelectedIndex, grassTypeBox.SelectedIndex, townLayerBox.Checked, 0, (long)uint.MaxValue + 1L, gpuAdapterBox.SelectedIndex == -1 ? null : (string)gpuAdapterBox.Items[gpuAdapterBox.SelectedIndex]); // 0xc7621027
            }

            if (possible_seeds != null)
            {
                TownGeneration gen = new();
                for (int i = 0; i < possible_seeds.Count; i++)
                {
                    gen.__qrand_idum = possible_seeds[i];
                    gen.fqrand_raw(); // fruit
                    gen.fqrand_raw(); // grass pattern
                    if (gen.GenerateBGOptimal(well_acre, lake_acre, police_acre, museum_acre)) // 39, 24, 36, 29
                    {
                        gen.__qrand_idum = possible_seeds[i];
                        // Do full generation now (we need to convert trees which use qrand calls)
                        ushort[] bg = gen.GenerateBG();
                        ushort[][] fg = gen.GenerateFG(bg, 3);
                        List<int[][]> data = new();

                        //Console.WriteLine();
                        if (gen.fqrand_raw() == seedToSearchFor) // 0xc7621027
                        {
                            Console.WriteLine($"GOT IT! {i} Seed: {possible_seeds[i]:X8}");

                            // 300 rng call spread start at 300
                            gen.__qrand_idum = result.TrainSeed; // 0x67548B1A //seeds[0];
                                                                 //for (int _ = 0; _ < 300; _++)
                            for (int _ = 0; _ < TownSeedRandCallsStartBox.Value; _++)
                            {
                                gen.fqrand_raw();
                            }

                            uint last_idum = gen.__qrand_idum;

                            //for (int x = 0; x < 300; x++)
                            for (int x = 0; x < TownFQRandCallSpreadBox.Value; x++)
                            {
                                gen.__qrand_idum = last_idum;
                                for (int z = 0; z < x; z++)
                                {
                                    gen.fqrand_raw();
                                }

                                // Create a copy of the fg array because this will modify it.
                                ushort[][] fg_copy = new ushort[fg.Length][];
                                for (int z = 0; z < fg_copy.Length; z++)
                                {
                                    ushort[] acre_copy = new ushort[16 * 16];
                                    Array.Copy(fg[z], acre_copy, 16 * 16);
                                    fg_copy[z] = acre_copy;
                                }

                                last_idum = gen.__qrand_idum;
                                gen.__qrand_idum = last_idum;
                                gen.GetShineSpotAndMoneyStonePositions(bg, fg_copy, out int[][] locs);

                                // testing
                                //if (locs[0][1] == 6 && locs[0][0] == 2)
                                //if (locs[1][1] == 3 && locs[1][0] == 4)
                                {
                                    data.Add(new int[][] { locs[0], locs[1], new int[] { x } });
                                    //Console.WriteLine(x);
                                }
                            }

                            new MapProbabilityForm(bg, gen.GetMapIcons(bg), data).Show();
                            break;
                        }
                    }

                    /*
                    if (possible_seeds[i] == 0x05a3e56e)
                    {
                        Console.WriteLine("Got it!");
                        gen.__qrand_idum = possible_seeds[i];
                        gen.GenerateBGOptimal(39, 24, 36, 29); // 24
                        gen.__qrand_idum = possible_seeds[i];
                        new MapProbabilityForm(gen.GetMapIcons(gen.GenerateBG()), new List<int[][]>()).Show();
                    }
                    */
                }
            }

            progressBar1.Style = ProgressBarStyle.Continuous;
            TownButton.Enabled = true;
        }

        private void townLayerBox_CheckedChanged(object sender, EventArgs e)
        {
            mTownStateChanged = true;
        }

        private void AcreListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mTownStateChanged = true;
        }
    }
}
