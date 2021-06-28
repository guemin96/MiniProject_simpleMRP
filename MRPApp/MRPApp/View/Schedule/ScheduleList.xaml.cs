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
using System.Windows.Input;

namespace MRPApp.View.Schedule
{
    /// <summary>
    /// MyAccount.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScheduleList : Page
    {
        public ScheduleList()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadGridData();
                LoadControlData();//콤보박스 데이터 로딩
                InitErrorMessages();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 StoreList Loaded : {ex}");
                throw ex;
            }
        }

        private void LoadControlData()
        {
            var plantCodes = Logic.DataAccess.GetSettings().Where(c => c.BasicCode.Contains("PC01")).ToList(); //PC01이 적힌 값들을 다 가져오는 쿼리문
            CboPlantCode.ItemsSource = plantCodes;
            CboGridPlantCode.ItemsSource = plantCodes;

            var facilityIds = Logic.DataAccess.GetSettings().Where(c => c.BasicCode.Contains("FAC1")).ToList();//FAC1이 적힌 값들을 다 가져오는 쿼리문
            CboSchFacilityID.ItemsSource = facilityIds;
            //진짜진짜 중요!!!!!!!!!!!!!! 다른 테이블의 값을 가져오는 역할
        }

        private void InitErrorMessages()
        {
            LblPlantCode.Visibility = LblSchAmount.Visibility = LblSchDate.Visibility =
                LblSchEndTime.Visibility = LblSchFacilityID.Visibility =
                LblSchLoadTime.Visibility = LblSchStartTime.Visibility = Visibility.Hidden;
        }
        private void LoadGridData()
        {
            List<Model.Schedules> list = Logic.DataAccess.GetSchedules();
            this.DataContext = list;
        }
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }
        private async void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            //if (GrdData.SelectedItem != null) ;
            if (IsValidInputs() != true) return;

            var item = new Model.Schedules();
            item.PlantCode = CboPlantCode.SelectedValue.ToString();
            item.SchDate = DateTime.Parse(DtpSchDate.Text);
            item.SchLoadTime = int.Parse(TxtSchLoadTime.Text);
            if (TmpSchStartTime.SelectedDateTime != null)
            {
                item.SchStartTime = TmpSchStartTime.SelectedDateTime.Value.TimeOfDay;
            }
            if (TmpSchEndTime.SelectedDateTime != null)
            {
                item.SchEndTime = TmpSchEndTime.SelectedDateTime.Value.TimeOfDay;
            }
            item.SchFacilityID = CboSchFacilityID.SelectedValue.ToString();
            item.SchAmount = (int)NudSchAmount.Value;

            item.RegDate = DateTime.Now;
            item.RegID = "MRP";

            try
            {
                var result = Logic.DataAccess.SetSchedule(item);
                if (result == 0)
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    Commons.ShowMessageAsync("오류", "데이터 수정실패!!");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 : {item.SchIdx}");
                    ClearInputs();
                    LoadGridData();//수정된 값 다시 불러오는 역할
                                   //var result = Logic.DataAccess.SetSettings(setting);


                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }
        //입력 데이터 검증 메서드
        private bool IsValidInputs()
        {
            var isValid = true;
            InitErrorMessages();

            if (CboPlantCode.SelectedValue == null)
            {
                LblPlantCode.Visibility = Visibility.Visible;
                LblPlantCode.Text = "공장을 선택하세요";
                isValid = false;
            }
            if (string.IsNullOrEmpty(DtpSchDate.Text))
            {
                LblSchDate.Visibility = Visibility.Visible;
                LblSchDate.Text = "공정일을 입력하세요";
                isValid = false;
            }
            try
            {
                var result = Logic.DataAccess.GetSchedules().Where(s => s.PlantCode.Equals(CboPlantCode.SelectedValue.ToString()))
                .Where(d => d.SchDate.Equals(DateTime.Parse(DtpSchDate.Text))).Count();
                if (result > 0)
                {
                    LblSchDate.Visibility = Visibility.Visible;
                    LblSchDate.Text = "해당 공장 공정일에 계획이 이미 있습니다.";
                    isValid = false;
                }
            }
            catch (Exception ex)
            {

            }
            //공장별로 공정일일 DB값이 있으면 입력되면 안됨
            //PC01001(수원) 2021-06-24

            if (string.IsNullOrEmpty(TxtSchLoadTime.Text))
            {
                LblSchLoadTime.Visibility = Visibility.Visible;
                LblSchLoadTime.Text = "로드타임을 입력하세요";
                isValid = false;
            }
            if (CboSchFacilityID.SelectedValue == null)
            {
                LblSchFacilityID.Visibility = Visibility.Visible;
                LblSchFacilityID.Text = "공정설비를 선택하세요";
                isValid = false;
            }
            if (NudSchAmount.Value <= 0)
            {
                LblSchAmount.Visibility = Visibility.Visible;
                LblSchAmount.Text = "수량을 입력하세요";
                isValid = false;
            }

            return isValid;

        }
        //수정 데이터 검증 메서드
        private bool IsValidUpdate()
        {
            var isValid = true;
            InitErrorMessages();

            if (CboPlantCode.SelectedValue == null)
            {
                LblPlantCode.Visibility = Visibility.Visible;
                LblPlantCode.Text = "공장을 선택하세요";
                isValid = false;
            }
            if (string.IsNullOrEmpty(DtpSchDate.Text))
            {
                LblSchDate.Visibility = Visibility.Visible;
                LblSchDate.Text = "공정일을 입력하세요";
                isValid = false;
            }
            
            //공장별로 공정일일 DB값이 있으면 입력되면 안됨
            //PC01001(수원) 2021-06-24

            if (string.IsNullOrEmpty(TxtSchLoadTime.Text))
            {
                LblSchLoadTime.Visibility = Visibility.Visible;
                LblSchLoadTime.Text = "로드타임을 입력하세요";
                isValid = false;
            }
            if (CboSchFacilityID.SelectedValue == null)
            {
                LblSchFacilityID.Visibility = Visibility.Visible;
                LblSchFacilityID.Text = "공정설비를 선택하세요";
                isValid = false;
            }
            if (NudSchAmount.Value <= 0)
            {
                LblSchAmount.Visibility = Visibility.Visible;
                LblSchAmount.Text = "수량을 입력하세요";
                isValid = false;
            }

            return isValid;

        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var search = DtpSearch.Text;
            var list = Logic.DataAccess.GetSchedules().Where(s => s.SchDate.Equals(DateTime.Parse(search))).ToList(); // list로 통일시켜서 복사해서 사용할때 일일이 바꾸지 않도록 작업
            this.DataContext = list;
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // if (GrdData.SelectedItem != null) ;
            if (IsValidUpdate() != true) return;

            var item = GrdData.SelectedItem as Model.Schedules;
            item.PlantCode = CboPlantCode.SelectedValue.ToString();
            item.SchDate = DateTime.Parse(DtpSchDate.Text);
            item.SchLoadTime = int.Parse(TxtSchLoadTime.Text);
            if (TmpSchStartTime.SelectedDateTime !=null)
            {
                item.SchStartTime = TmpSchStartTime.SelectedDateTime.Value.TimeOfDay;
            }
            if (TmpSchEndTime.SelectedDateTime !=null)
            {
                item.SchEndTime = TmpSchEndTime.SelectedDateTime.Value.TimeOfDay;
            }
            item.SchFacilityID = CboSchFacilityID.SelectedValue.ToString();
            item.SchAmount = (int)NudSchAmount.Value;

            item.ModDate = DateTime.Now;
            item.ModID = "MRP";

            try
            {
                var result = Logic.DataAccess.SetSchedule(item);
                if (result == 0)
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    Commons.ShowMessageAsync("오류", "데이터 수정실패!!");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 : {item.SchIdx}");
                    ClearInputs();
                    LoadGridData();//수정된 값 다시 불러오는 역할
                                   //var result = Logic.DataAccess.SetSettings(setting);


                    //}
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }
        private void ClearInputs()
        {
            TxtSchIdx.Text = "";
            CboPlantCode.SelectedItem = null;
            DtpSchDate.Text = "";
            TxtSchLoadTime.Text = "";
            TmpSchStartTime.SelectedDateTime = null;
            TmpSchEndTime.SelectedDateTime = null;
            CboSchFacilityID.SelectedItem = null;
            NudSchAmount.Value = 0;

            CboPlantCode.Focus();
        }
        private void GrdData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            ClearInputs();
            try
            {
                var item = GrdData.SelectedItem as Model.Schedules;
                TxtSchIdx.Text = item.SchIdx.ToString();
                CboPlantCode.SelectedValue = item.PlantCode;
                DtpSchDate.Text = item.SchDate.ToString();
                TxtSchLoadTime.Text = item.SchLoadTime.ToString();
                if (item.SchStartTime!=null)
                {
                    TmpSchStartTime.SelectedDateTime = new DateTime(item.SchStartTime.Value.Ticks);
                }
                if (item.SchEndTime!= null)
                {
                    TmpSchEndTime.SelectedDateTime = new DateTime(item.SchEndTime.Value.Ticks);
                }
                CboSchFacilityID.SelectedValue = item.SchFacilityID;
                NudSchAmount.Value = item.SchAmount;
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외 발생 : {ex}");
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var setting = GrdData.SelectedItem as Model.Settings;

            if (setting == null)
            {
                await Commons.ShowMessageAsync("삭제", "삭제할 코드를 선택하시오");
                return;
            }
            else
            {
                try
                {
                    var result = Logic.DataAccess.DelSetting(setting);
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
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnSearch_Click(sender, e);
        }
    }
}
