using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace AC_fqrand_solver
{
    public partial class Form1 : Form
    {
        private TextBox[] turnipBoxes;

        public Form1()
        {
            InitializeComponent();
            turnipBoxes = new TextBox[]{ 
                txtTurnipSundayPrice, txtTurnipMondayPrice, txtTurnipTuesdayPrice, txtTurnipWednesdayPrice,
                txtTurnipThursdayPrice, txtTurnipFridayPrice, txtTurnipSaturdayPrice
            };
            cmbNookShop.SelectedIndex = 0;
        }

        private void btnfqrandCalc_Click(object sender, EventArgs e)
        {
            float __qrand_itemp = Logic.fqrand();

            txtqrand_idum.Text = $"0x{Logic.__qrand_idum:X8}";
            txtqrand_itemp.Text = Convert.ToString(__qrand_itemp);
        }

        private void btnInitialize_qrand_idum_Click(object sender, EventArgs e)
        {
            Logic.SetSeed();
            txtqrand_idum.Text = $"0x{Logic.__qrand_idum:X8}";
        }

        private void GetListPriorities(out ListPriority[] listAPrios, out ListPriority[] listBPrios, out ListPriority[] listCPrios)
        {
            listAPrios = new ListPriority[5];
            listBPrios = new ListPriority[5];
            listCPrios = new ListPriority[5];

            switch (cmbFurnitureListCommon.Text)
            {
                case "List A": listAPrios[0] = ListPriority.Common; break;
                case "List B": listBPrios[0] = ListPriority.Common; break;
                case "List C": listCPrios[0] = ListPriority.Common; break;
            }
            switch (cmbFurnitureListUncommon.Text)
            {
                case "List A": listAPrios[0] = ListPriority.Uncommon; break;
                case "List B": listBPrios[0] = ListPriority.Uncommon; break;
                case "List C": listCPrios[0] = ListPriority.Uncommon; break;
            }
            switch (cmbFurnitureListRare.Text)
            {
                case "List A": listAPrios[0] = ListPriority.Rare; break;
                case "List B": listBPrios[0] = ListPriority.Rare; break;
                case "List C": listCPrios[0] = ListPriority.Rare; break;
            }

            switch (cmbStationeryListCommon.Text)
            {
                case "List A": listAPrios[1] = ListPriority.Common; break;
                case "List B": listBPrios[1] = ListPriority.Common; break;
                case "List C": listCPrios[1] = ListPriority.Common; break;
            }
            switch (cmbStationeryListUncommon.Text)
            {
                case "List A": listAPrios[1] = ListPriority.Uncommon; break;
                case "List B": listBPrios[1] = ListPriority.Uncommon; break;
                case "List C": listCPrios[1] = ListPriority.Uncommon; break;
            }
            switch (cmbStationeryListRare.Text)
            {
                case "List A": listAPrios[1] = ListPriority.Rare; break;
                case "List B": listBPrios[1] = ListPriority.Rare; break;
                case "List C": listCPrios[1] = ListPriority.Rare; break;
            }

            switch (cmbClothingListCommon.Text)
            {
                case "List A": listAPrios[2] = ListPriority.Common; break;
                case "List B": listBPrios[2] = ListPriority.Common; break;
                case "List C": listCPrios[2] = ListPriority.Common; break;
            }
            switch (cmbClothingListUncommon.Text)
            {
                case "List A": listAPrios[2] = ListPriority.Uncommon; break;
                case "List B": listBPrios[2] = ListPriority.Uncommon; break;
                case "List C": listCPrios[2] = ListPriority.Uncommon; break;
            }
            switch (cmbClothingListRare.Text)
            {
                case "List A": listAPrios[2] = ListPriority.Rare; break;
                case "List B": listBPrios[2] = ListPriority.Rare; break;
                case "List C": listCPrios[2] = ListPriority.Rare; break;
            }

            switch (cmbCarpetListCommon.Text)
            {
                case "List A": listAPrios[3] = ListPriority.Common; break;
                case "List B": listBPrios[3] = ListPriority.Common; break;
                case "List C": listCPrios[3] = ListPriority.Common; break;
            }
            switch (cmbCarpetListUncommon.Text)
            {
                case "List A": listAPrios[3] = ListPriority.Uncommon; break;
                case "List B": listBPrios[3] = ListPriority.Uncommon; break;
                case "List C": listCPrios[3] = ListPriority.Uncommon; break;
            }
            switch (cmbCarpetListRare.Text)
            {
                case "List A": listAPrios[3] = ListPriority.Rare; break;
                case "List B": listBPrios[3] = ListPriority.Rare; break;
                case "List C": listCPrios[3] = ListPriority.Rare; break;
            }

            switch (cmbWallpaperListCommon.Text)
            {
                case "List A": listAPrios[4] = ListPriority.Common; break;
                case "List B": listBPrios[4] = ListPriority.Common; break;
                case "List C": listCPrios[4] = ListPriority.Common; break;
            }
            switch (cmbWallpaperListUncommon.Text)
            {
                case "List A": listAPrios[4] = ListPriority.Uncommon; break;
                case "List B": listBPrios[4] = ListPriority.Uncommon; break;
                case "List C": listCPrios[4] = ListPriority.Uncommon; break;
            }
            switch (cmbWallpaperListRare.Text)
            {
                case "List A": listAPrios[4] = ListPriority.Rare; break;
                case "List B": listBPrios[4] = ListPriority.Rare; break;
                case "List C": listCPrios[4] = ListPriority.Rare; break;
            }
        }

        private void ShowErrorMessage(in string msg)
        {
            MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void btnSetItems_Click(object sender, EventArgs e)
        {
            int goodsPower = int.Parse(txtGoodsPower.Text);
            int day = dateCurrentDate.Value.Day;
            int month = dateCurrentDate.Value.Month;
            int year = dateCurrentDate.Value.Year;

            GetListPriorities(out var listAPrios, out var listBPrios, out var listCPrios);

            if (cmbNookShop.SelectedIndex < 1 || cmbNookShop.SelectedIndex > 4)
            {
                ShowErrorMessage("You must set a valid Nook's Shop Level!");
                return;
            }

            Logic.ShopType shopType = (Logic.ShopType)(cmbNookShop.SelectedIndex - 1);
            ushort[] nookFurnitureList = null;
            ushort rareItem = 0;
            
            // TODO: Nook's Cranny & Nook 'n' Go
            switch (shopType)
            {
                case Logic.ShopType.Cranny:
                    {
                        break;
                    }
                case Logic.ShopType.Nook_n_Go:
                    {
                        nookFurnitureList = new ushort[2];
                        break;
                    }
                case Logic.ShopType.Nookway:
                    {
                        nookFurnitureList = new ushort[3];
                        if (ushort.TryParse(txtNookFurniture1.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture1))
                            rareItem = valNookFurniture1;
                        if (ushort.TryParse(txtNookFurniture2.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture2))
                            nookFurnitureList[0] = valNookFurniture2;
                        if (ushort.TryParse(txtNookFurniture3.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture3))
                            nookFurnitureList[1] = valNookFurniture3;
                        if (ushort.TryParse(txtNookFurniture4.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture4))
                            nookFurnitureList[2] = valNookFurniture4;
                        break;
                    }
                case Logic.ShopType.Nookingtons:
                    {
                        nookFurnitureList = new ushort[5];
                        if (ushort.TryParse(txtNookFurniture1.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture1))
                            rareItem = valNookFurniture1;
                        if (ushort.TryParse(txtNookFurniture2.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture2))
                            nookFurnitureList[0] = valNookFurniture2;
                        if (ushort.TryParse(txtNookFurniture3.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture3))
                            nookFurnitureList[1] = valNookFurniture3;
                        if (ushort.TryParse(txtNookFurniture4.Text, System.Globalization.NumberStyles.HexNumber, null, out var valNookFurniture4))
                            nookFurnitureList[2] = valNookFurniture4;
                        break;
                    }
            }

            var now = DateTime.Now;
            resultsBox.Clear();
            var progressHandler = new Progress<uint>(value =>
            {
                resultsBox.Text += $"{value:X8}\n";
            });
            await Logic.ThreadedBruteForceNookwayNookingtons(shopType, listAPrios[0], listBPrios[0], listCPrios[3], goodsPower, rareItem, nookFurnitureList, progressHandler);
            var end = DateTime.Now;
            MessageBox.Show($"Don searching! Took {end.Subtract(now).TotalSeconds} seconds!", "Idum Value Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void chkBillionTownSettings_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBillionTownSettings.Checked == true)
            {
                cmbFurnitureListCommon.Text = "List A";
                cmbFurnitureListUncommon.Text = "List C";
                cmbFurnitureListRare.Text = "List B";
                cmbStationeryListCommon.Text = "List A";
                cmbStationeryListUncommon.Text = "List C";
                cmbStationeryListRare.Text = "List B";
                cmbClothingListCommon.Text = "List B";
                cmbClothingListUncommon.Text = "List C";
                cmbClothingListRare.Text = "List A";
                cmbCarpetListCommon.Text = "List A";
                cmbCarpetListUncommon.Text = "List C";
                cmbCarpetListRare.Text = "List B";
                cmbWallpaperListCommon.Text = "List C";
                cmbWallpaperListUncommon.Text = "List B";
                cmbWallpaperListRare.Text = "List A";
                cmbNookShop.SelectedIndex = 4;
                txtGoodsPower.Text = "0";

                //Remove after finishing program
                txtNookFurniture1.Text = "134C";
                txtNookFurniture2.Text = "1124";
                txtNookFurniture3.Text = "1298";
                txtNookFurniture4.Text = "3200";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int goodsPower = int.Parse(txtGoodsPower.Text);
            int day = dateCurrentDate.Value.Day;
            int month = dateCurrentDate.Value.Month;
            GetListPriorities(out var listAPrios, out var listBPrios, out var listCPrios);
            var stock = Logic.GenerateNookwayStock(listAPrios, listBPrios, listCPrios, goodsPower, month, day);
            txtNookFurniture1.Text = stock[0].ToString("X4");
            txtNookFurniture2.Text = stock[1].ToString("X4");
            txtNookFurniture3.Text = stock[2].ToString("X4");
            txtNookFurniture4.Text = stock[3].ToString("X4");

            txtNookStationery1.Text = stock[4].ToString("X4");
            txtNookStationery2.Text = stock[5].ToString("X4");

            txtNookDiary.Text = stock[6].ToString("X4");

            txtNookClothes1.Text = stock[7].ToString("X4");
            txtNookClothes2.Text = stock[8].ToString("X4");
            txtNookClothes3.Text = stock[9].ToString("X4");

            txtNookCarpet1.Text = stock[10].ToString("X4");
            txtNookCarpet2.Text = stock[11].ToString("X4");

            txtNookWallpaper1.Text = stock[12].ToString("X4");
            txtNookWallpaper2.Text = stock[13].ToString("X4");

            txtNookTool1.Text = stock[14].ToString("X4");
            txtNookTool2.Text = stock[15].ToString("X4");
            txtNookUmbrella.Text = stock[16].ToString("X4");

            // 17 & 18 = saplings

            txtNookFlower1.Text = stock[19].ToString("X4");
            txtNookFlower2.Text = stock[20].ToString("X4");
            txtNookFlower3.Text = stock[21].ToString("X4");
            txtNookFlower4.Text = stock[22].ToString("X4");

            // Roll random 4 times for deterministic functions
            for (var i = 0; i < fqrandCallBox.Value; i++)
                Logic.fqrand();
            var currentTrend = cmbPreviousTurnipTrend.Text switch
            {
                "Spike" => MarketTrend.Spike,
                "Random" => MarketTrend.Random,
                "Falling" => MarketTrend.Falling,
                _ => throw new ArgumentOutOfRangeException(nameof(cmbPreviousTurnipTrend.Text)),
            };
            ushort[] turnipPrices = Logic.DecidePriceSchedule(currentTrend, out var newTrend);
            for (var i = 0; i < 7; i++)
            {
                turnipBoxes[i].Text = turnipPrices[i].ToString();
            }
            txtTurnipTrend.Text = newTrend.ToString();
        }
    }
}
