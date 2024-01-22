using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AC_fqrand_solver
{
    public partial class MapProbabilityForm : Form
    {
        private const int MAP_SIZE = 22;
        private const int SCALE = 6;
        private const double FONT_SCALE = 2.0d;
        private const int MAP_SCALE_SIZE = MAP_SIZE * SCALE;
        private const float MAP_UNIT_SIZE = (MAP_SCALE_SIZE) / 16.0f;

        private RectangleF highlightRect = new(0, 0, MAP_UNIT_SIZE, MAP_UNIT_SIZE);
        private Point selected = new(-1, -1);
        private PointF selectedUt = new(-1, -1);
        private Point lastSelected = new(-1, -1);
        private PointF lastSelectedUt = new(-1, -1);
        private bool in_box = false;

        private readonly ushort[] m_bg;
        private readonly int[,] m_map_tiles;
        private readonly List<int[][]> m_shine_rock_data;
        private List<int[][]> m_moded_shine_rock_data;

        private bool set_shine_spot = false;
        private bool set_rock_spot = false;

        public MapProbabilityForm(in ushort[] bg, in int[,] map_tiles, in List<int[][]> shine_rock_data)
        {
            InitializeComponent();
            m_bg = bg;
            m_map_tiles = map_tiles;
            m_shine_rock_data = shine_rock_data;
            m_moded_shine_rock_data = new(shine_rock_data);
            LoadMap(map_tiles, GetPercentages(shine_rock_data));
        }

        private (double[,], double[,], List<int[]>[,], List<int[]>[,]) GetPercentages(in List<int[][]> shine_rock_data)
        {
            List<int[]>[,] shines_by_acre = new List<int[]>[6,5];
            List<int[]>[,] rocks_by_acre = new List<int[]> [6,5];
            double[,] shine_percents = new double[6,5];
            double[,] rock_percents = new double[6,5];

            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    shines_by_acre[y, x] = new List<int[]>();
                    rocks_by_acre[y, x] = new List<int[]>();
                }
            }

            // data[0] = shine, data[1] = rock
            foreach (int[][] data in shine_rock_data)
            {
                shine_percents[data[0][1] - 1, data[0][0] - 1]++;
                rock_percents[data[1][1] - 1, data[1][0] - 1]++;

                shines_by_acre[data[0][1] - 1, data[0][0] - 1].Add(new int[2] { data[0][2], data[0][3] });
                rocks_by_acre[data[1][1] - 1, data[1][0] - 1].Add(new int[2] { data[1][2], data[1][3] });
            }

            for(int z = 0; z < 6; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    shine_percents[z, x] /= shine_rock_data.Count;
                    rock_percents[z, x] /= shine_rock_data.Count;
                }
            }

            return (shine_percents, rock_percents, shines_by_acre, rocks_by_acre);
        }

        private void LoadMap(in int[,] map_tiles, in (double[,], double[,], List<int[]>[,], List<int[]>[,]) percents)
        {
            if (MapPictureBox.Image != null)
            {
                Image i = MapPictureBox.Image;
                MapPictureBox.Image = null;
                i.Dispose();
            }

            MapPictureBox.Size = new Size((MAP_SIZE * 5) * SCALE, (MAP_SIZE * 6) * SCALE);
            Bitmap map = new((MAP_SIZE * 5) * SCALE, (MAP_SIZE * 6) * SCALE);

            using Font font = new("Arial", (int)(12 * FONT_SCALE), FontStyle.Bold);
            using Pen outlinePen = new(Brushes.Black, 2);
            outlinePen.LineJoin = LineJoin.Round;
            using FontFamily family = new("Arial");
            using Graphics g = Graphics.FromImage(map);

            using Brush maxShineBrush = new SolidBrush(Color.FromArgb(80, Color.Yellow));
            using Brush maxRockBrush = new SolidBrush(Color.FromArgb(80, Color.HotPink));
            using Brush maxBothBrush = new SolidBrush(Color.FromArgb(80, 255, 180, 90));

            using Brush rockBrush = new SolidBrush(Color.Black);
            using Brush moneyRockBrush = new SolidBrush(Color.Red);
            using Brush shineSpotBrush = new SolidBrush(Color.Orange);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            StringFormat shine_format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            StringFormat rock_format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Far
            };

            // Determine maximum acres for both golden spot & money rock
            List<int> maxShineIdxs = new() { 0 };
            List<int> maxRockIdxs = new() { 0 };
            double maxShineVal = percents.Item1[0, 0];
            double maxRockVal = percents.Item2[0, 0];

            for (int i = 1; i < 30; i++)
            {
                double shineVal = percents.Item1[i / 5, i % 5];
                if (shineVal > maxShineVal)
                {
                    maxShineIdxs = new() { i };
                    maxShineVal = shineVal;
                }
                else if (shineVal == maxShineVal)
                {
                    maxShineIdxs.Add(i);
                }

                double rockVal = percents.Item2[i / 5, i % 5];
                if (rockVal > maxRockVal)
                {
                    maxRockIdxs = new() { i };
                    maxRockVal = rockVal;
                }
                else if (rockVal == maxRockVal)
                {
                    maxRockIdxs.Add(i);
                }
            }

            int square_size = MAP_SCALE_SIZE + (SCALE >> 1);

            for (int i = 0; i < 30; i++)
            {
                int x = i % 5;
                int z = i / 5;

                int pixel_x = MAP_SCALE_SIZE * x;
                int pixel_y = MAP_SCALE_SIZE * z;

                bool icon = !in_box || selected.X != x || selected.Y != z;

                string img_path = icon ? $"AC_fqrand_solver.Resources.MapIcon_{map_tiles[z, x]}.png" : $"AC_fqrand_solver.Resources.MapImages.{(m_bg[(z + 1) * 7 + x + 1] & ~3):X4}.jpg";
                using Stream image_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(img_path);
                using Image img = Image.FromStream(image_stream);

                int sz = icon ? square_size : MAP_SCALE_SIZE;
                //Console.WriteLine($"{x}-{z} -> x: {pixel_x} y: {pixel_y} w: {square_size} h: {square_size}");
                Rectangle dst_rect = new(pixel_x, pixel_y, square_size, square_size);
                g.DrawImage(img, new Rectangle(pixel_x, pixel_y, sz, sz));

                // Don't draw text if it's zero

                if (maxShineIdxs.Contains(i))
                {
                    if (maxRockIdxs.Contains(i))
                    {
                        g.FillRectangle(maxBothBrush, dst_rect);
                    }
                    else
                    {
                        g.FillRectangle(maxShineBrush, dst_rect);
                    }
                }
                else if (maxRockIdxs.Contains(i))
                {
                    g.FillRectangle(maxRockBrush, dst_rect);
                }

                // Draw shine %
                if (percents.Item1[z, x] != 0)
                {
                    using GraphicsPath path = new();
                    path.AddString($"{percents.Item1[z, x] * 100:0.#}%", family, (int)FontStyle.Bold, (int)(12 * FONT_SCALE), dst_rect, shine_format);
                    g.DrawPath(outlinePen, path);
                    g.FillPath(Brushes.Gold, path);
                }

                // Draw rock %
                if (percents.Item2[z, x] != 0)
                {
                    using GraphicsPath path2 = new();
                    path2.AddString($"{percents.Item2[z, x] * 100:0.#}%", family, (int)FontStyle.Bold, (int)(12 * FONT_SCALE), dst_rect, rock_format);
                    g.DrawPath(outlinePen, path2);
                    g.FillPath(Brushes.HotPink, path2);
                }

                // Draw shines & rocks
                List<int[]> shines = percents.Item3[z, x];
                List<int[]> rocks = percents.Item4[z, x];

                if (showGoldenBox.Checked)
                {
                    foreach (int[] shine in shines)
                    {
                        g.FillRectangle(shineSpotBrush, pixel_x + MAP_UNIT_SIZE * shine[0], pixel_y + MAP_UNIT_SIZE * shine[1], MAP_UNIT_SIZE, MAP_UNIT_SIZE);
                    }
                }

                if (showRockBox.Checked)
                {
                    foreach (int[] rock in rocks)
                    {
                        g.FillRectangle(rockBrush, pixel_x + MAP_UNIT_SIZE * rock[0], pixel_y + MAP_UNIT_SIZE * rock[1], MAP_UNIT_SIZE, MAP_UNIT_SIZE);
                    }
                }
            }

            ShineLabel.Text = "Best Golden Spot Acre(s): " + maxShineIdxs.Aggregate("", (curr, s) => curr += $"{(char)('A' + s / 5)}-{(s % 5) + 1}, ");
            ShineLabel.Text = ShineLabel.Text.Substring(0, ShineLabel.Text.Length - 2);
            MoneyRockLabel.Text = "Best Money Rock Acre(s): " + maxRockIdxs.Aggregate("", (curr, s) => curr += $"{(char)('A' + s / 5)}-{(s % 5) + 1}, ");
            MoneyRockLabel.Text = MoneyRockLabel.Text.Substring(0, MoneyRockLabel.Text.Length - 2);
            MapPictureBox.Image = map;
        }

        private static readonly Brush utBrush = new SolidBrush(Color.FromArgb(196, Color.Yellow));
        private static readonly Brush acreBrush = new SolidBrush(Color.FromArgb(128, Color.Yellow));

        private void MapPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (in_box)
            {
                e.Graphics.FillRectangle(AcreModeCheckBox.Checked ? acreBrush : utBrush, highlightRect);
            }
        }

        private void MapPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point loc = e.Location;
            selected = new Point(loc.X / MAP_SCALE_SIZE, loc.Y / MAP_SCALE_SIZE);

            selectedUt = new PointF((float)Math.Floor(loc.X / MAP_UNIT_SIZE), (float)Math.Floor(loc.Y / MAP_UNIT_SIZE));

            //Console.WriteLine(loc);
            //Console.WriteLine($"{loc} -> {selectedUt} -> {new PointF(selectedUt.X * MAP_UNIT_SIZE, selectedUt.Y * MAP_UNIT_SIZE)} | {highlightRect.Width}x{highlightRect.Height}");
            if (selectedUt != lastSelectedUt)
            {
                if (!AcreModeCheckBox.Checked)
                {
                    highlightRect.Location = new PointF(selectedUt.X * MAP_UNIT_SIZE, selectedUt.Y * MAP_UNIT_SIZE);
                }

                lastSelectedUt = selectedUt;
            }

            if (lastSelected != selected)
            {
                if (!in_box)
                {
                    in_box = true;
                }

                if (AcreModeCheckBox.Checked)
                {
                    highlightRect.Location = new PointF(selected.X * MAP_SCALE_SIZE, selected.Y * MAP_SCALE_SIZE);
                }

                lastSelected = selected;
                LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
                MapPictureBox.Refresh();
            }
            else
            {
                in_box = true;
                MapPictureBox.Refresh();
            }
        }

        private void MapPictureBox_MouseLeave(object sender, EventArgs e)
        {
            in_box = false;
            lastSelectedUt = lastSelected = new Point(-1, -1);
            LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
            MapPictureBox.Refresh();
        }

        private void MapPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (in_box)
            {
                int z = selected.Y + 1;
                int x = selected.X + 1;

                switch (e.Button)
                {
                    case MouseButtons.Left:
                        {
                            // Check if undo combo
                            if (ModifierKeys.HasFlag(Keys.Shift))
                            {
                                // Find all ids with the correct acre
                                List<int> ids = new();
                                foreach (var source in m_shine_rock_data)
                                {
                                    if ((source[0][1] == z && source[0][0] == x) || (source[1][1] == z && source[1][0] == x))
                                    {
                                        ids.Add(source[2][0]);
                                    }
                                }

                                // Remove any acres still in the current data
                                foreach (var dst in m_moded_shine_rock_data)
                                {
                                    if (ids.Contains(dst[2][0]))
                                    {
                                        ids.Remove(dst[2][0]);
                                    }
                                }

                                // Add removed data back in
                                foreach (var id in ids)
                                {
                                    m_moded_shine_rock_data.Add(m_shine_rock_data[id]);
                                }

                                LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
                                set_shine_spot = false;
                                set_rock_spot = false;
                                return;
                            }

                            if (set_shine_spot) return;
                            set_shine_spot = true;

                            // Found shine spot
                            for (int i = m_moded_shine_rock_data.Count - 1; i >= 0 ; i--)
                            {
                                int[] shine_data = m_moded_shine_rock_data[i][0];
                                if (shine_data[1] != z || shine_data[0] != x)
                                {
                                    m_moded_shine_rock_data.RemoveAt(i);
                                }
                            }

                            LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
                            break;
                        }
                    case MouseButtons.Middle:
                        {
                            if (set_rock_spot) return;
                            set_rock_spot = true;

                            // Found money rock
                            for (int i = m_moded_shine_rock_data.Count - 1; i >= 0; i--)
                            {
                                int[] rock_data = m_moded_shine_rock_data[i][1];
                                if (rock_data[1] != z || rock_data[0] != x)
                                {
                                    m_moded_shine_rock_data.RemoveAt(i);
                                }
                            }

                            LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
                            break;
                        }
                    case MouseButtons.Right:
                        {
                            // Money rock
                            if (ModifierKeys.HasFlag(Keys.Shift))
                            {
                                for (int i = m_moded_shine_rock_data.Count - 1; i >= 0; i--)
                                {
                                    int[] rock_data = m_moded_shine_rock_data[i][1];
                                    if (rock_data[1] == z && rock_data[0] == x)
                                    {
                                        m_moded_shine_rock_data.RemoveAt(i);
                                    }
                                }
                            }
                            else
                            {
                                // Shine spot
                                for (int i = m_moded_shine_rock_data.Count - 1; i >= 0; i--)
                                {
                                    int[] shine_data = m_moded_shine_rock_data[i][0];
                                    if (shine_data[1] == z && shine_data[0] == x)
                                    {
                                        m_moded_shine_rock_data.RemoveAt(i);
                                    }
                                }
                            }

                            LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
                            break;
                        }
                }
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            m_moded_shine_rock_data = new List<int[][]>(m_shine_rock_data);
            LoadMap(m_map_tiles, GetPercentages(m_shine_rock_data));
            set_rock_spot = false;
            set_shine_spot = false;
        }

        private void ShowCheckboxChanged(object sender, EventArgs e)
        {
            LoadMap(m_map_tiles, GetPercentages(m_moded_shine_rock_data));
        }

        private void AcreModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            highlightRect = AcreModeCheckBox.Checked ?
                new RectangleF(selected.X * MAP_SCALE_SIZE, selected.Y * MAP_SCALE_SIZE, MAP_SCALE_SIZE, MAP_SCALE_SIZE) :
                new RectangleF(selectedUt.X * MAP_UNIT_SIZE, selectedUt.Y * MAP_UNIT_SIZE, MAP_UNIT_SIZE, MAP_UNIT_SIZE);
        }
    }
}
