using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MRPApp.View.Setting
{
    /// <summary>
    /// MyAccount.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingList : Page
    {
        public SettingList()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadGridData();
                LblBasicCode.Visibility = LblCodeDesc.Visibility = LblCodeName.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 StoreList Loaded : {ex}");
                throw ex;
            }
        }

        private void LoadGridData()
        {
            List<Model.Settings> settings = Logic.DataAccess.GetSettings();
            this.DataContext = settings;
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //NavigationService.Navigate(new EditUser());
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnEditUser_Click : {ex}");
                throw ex;
            }
        }

        private void BtnAddStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               // NavigationService.Navigate(new AddStore());
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnAddStore_Click : {ex}");
                throw ex;
            }
        }

        private void BtnEditStore_Click(object sender, RoutedEventArgs e)
        {
            if (GrdData.SelectedItem == null)
            {
                Commons.ShowMessageAsync("창고수정", "수정할 창고를 선택하세요");
                return;
            }

            try
            {
                //var storeId = (GrdData.SelectedItem as Model.Store).StoreID;
                //NavigationService.Navigate(new EditStore(storeId));
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnEditStore_Click : {ex}");
                throw ex;
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            var setting = new Model.Settings();
            setting.BasicCode = TxtBasicCode.Text;
            setting.CodeName = TxtCodeName.Text;
            setting.CodeDesc = TxtCodeDesc.Text;
            try
            {
                var result = Logic.DataAccess.SetSettings(setting);
                if (result == 0)
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    await Commons.ShowMessageAsync("오류", "데이터 입력실패!!");//등록일 수정하기 다음시간!
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 : {setting.BasicCode}");
                    ClearInputs();
                    LoadGridData();//수정된 값 다시 불러오는 역할

                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var setting = GrdData.SelectedItem as Model.Settings;
            setting.CodeName = TxtCodeName.Text;
            setting.CodeDesc = TxtCodeDesc.Text;
            try
            {
                var result = Logic.DataAccess.SetSettings(setting);
                if (result == 0)
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    Commons.ShowMessageAsync("오류", "데이터 수정실패!!");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 : {setting.BasicCode}");
                    ClearInputs();
                    LoadGridData();//수정된 값 다시 불러오는 역할
                    
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }

        private void ClearInputs()
        {
            TxtBasicCode.IsReadOnly = false;
            TxtBasicCode.Background = new SolidColorBrush(Colors.White);
            TxtBasicCode.Text = TxtCodeDesc.Text = TxtCodeName.Text = string.Empty;//""
        }

        private void GrdData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                var setting = GrdData.SelectedItem as Model.Settings;
                TxtBasicCode.Text = setting.BasicCode;
                TxtCodeDesc.Text = setting.CodeDesc;
                TxtCodeName.Text = setting.CodeName;

                TxtBasicCode.IsReadOnly = true;
                TxtBasicCode.Background = new SolidColorBrush(Colors.LightGray);
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외 발생 : {ex}");
            }
        }
    }
}
