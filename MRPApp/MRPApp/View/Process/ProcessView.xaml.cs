﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MRPApp.View.Process
{
    /// <summary>
    /// ProcessView.xaml에 대한 상호 작용 논리
    /// 1. 공정계획에서 오늘의 생산계획 일정 불러옴
    /// 2. 없으면 에러표시, 시작버튼 클릭하지 못하게 만듬
    /// 3. 있으면 오늘의 날짜를 표시, 시작버튼 활성화
    /// 3-1. Mqtt Subscription 연결 factory1/machine1/data 확인...
    /// 4. 시작버튼 클릭시 새 공정을 생성, DB에 입력
    ///    공정코드 :  PRC20210618001 (PRC+yyyy+MM+dd+NNN)
    /// 5. 공정처리 애니메이션 시작
    /// 6. 로드타임 후 애니메이션 중지
    /// 7. 센서링값 리턴될 때까지 대기
    /// 8. 센서링 결과값에 따라서 생산품 색상 변경
    /// 9. 현재 공정의 DB값 업데이트
    /// 10. 결과 레이블 값 수정/표시
    /// </summary>
    public partial class ProcessView : Page
    {
        //금일 일정
        private Model.Schedules currSchedule;
        public ProcessView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                currSchedule = Logic.DataAccess.GetSchedules().Where(s => s.PlantCode.Equals(Commons.PLANTCODE))
                                .Where(s => s.SchDate.Equals(DateTime.Parse(today))).FirstOrDefault();
                if (currSchedule==null)
                {
                    await Commons.ShowMessageAsync("공정", "공정계획이 없습니다. 계획일정을 먼저 입력하세요");
                    //TODO : 시작버튼 Disable
                    LblProcessDate.Content = string.Empty;
                    LblSchLoadTime.Content = "None";
                    LblSchAmount.Content = "None";
                    BtnStartProcess.IsEnabled = false;

                    return;
                }
                else
                {
                    //공정계획 표시
                    await Commons.ShowMessageAsync("공정", $"{today} 공정 시작합니다.");
                    LblProcessDate.Content = currSchedule.SchDate.ToString("yyyy-MM-dd");
                    LblSchLoadTime.Content = $"{currSchedule.SchLoadTime}초";
                    LblSchAmount.Content = $"{currSchedule.SchAmount}개";
                    BtnStartProcess.IsEnabled = true;

                    InitConnectMqttBroker();// 공정 시작시 MQTT 브로커에 연결
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 MyAccount Loaded : {ex}");
                throw ex;
            }
        }
        MqttClient client;
        Timer timer;
        Stopwatch sw;

        private void InitConnectMqttBroker()
        {
            var brokerAddress = IPAddress.Parse("210.119.12.94"); // MQTT Mosquitto Broker IP
            client = new MqttClient(brokerAddress);
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.Connect("Monitor");
            client.Subscribe(new string[] { "factory1/machine1/data" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed; ;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sw.Elapsed.Seconds>=2)
            {
                sw.Stop();
                sw.Reset();
                MessageBox.Show("currentData[PRC_MSG]");
            }
        }
        Dictionary<string, string> currentData = new Dictionary<string, string>();

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            var currentData = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

            sw.Stop();
            sw.Reset();
            sw.Start();

            StartSensorAnimation();

        }

        private void StartSensorAnimation()
        {
            DoubleAnimation ba = new DoubleAnimation();
            ba.From = 1; //이미지 보임
            ba.To = 0; // 이미지 보이지 않음
            ba.Duration = TimeSpan.FromSeconds(2);
            ba.AutoReverse = true;
            //ba.RepeatBehavior = RepeatBehavior.Forever;

            Sensor.BeginAnimation(Canvas.OpacityProperty, ba); // 투명도 조절
        }

        private void StartProcess_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private bool InsertProcessData()
        {
            var item = new Model.Process();
            item.SchIdx = currSchedule.SchIdx;
            item.PrcCd = GetProcessCodeFromDB();
            item.PrcDate = DateTime.Now;
            item.PrcLoadTime = currSchedule.SchLoadTime;
            item.PrcStartTime = currSchedule.SchStartTime;
            item.PrcEndTime = currSchedule.SchEndTime;
            item.PrcFacilityID = Commons.FACILITYID;
            item.PrcResult = true; // 공정성공 일단 픽스
            item.RegDate = DateTime.Now;
            item.RegID = "MRP";
            try
            {
                var result = Logic.DataAccess.SetProcess(item);
                if (result == 0)
                {
                    Commons.LOGGER.Error("공정데이터 입력 실패!");
                    Commons.ShowMessageAsync("오류", "공정시작 오류발생, 관리자 문의");
                    return false;
                }
                else
                {
                    Commons.LOGGER.Info("공정데이터 입력!");
                    return true;

                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 : {ex}");
                Commons.ShowMessageAsync("공정 오류", "공정시작 오류 발생, 관리자 문의");
                return false;

            }
        }

        private string GetProcessCodeFromDB()
        {
            var prefix = "PRC";
            var prePrcCode = prefix + DateTime.Now.ToString("yyyyMMdd");
            var resultCode = string.Empty;
            // 이전까지 공정이 없어 (PRC20210629.. )null이 넘어오고
            // PRC20210620001,002,003,004-->PRC20210629004
            var maxPrc = Logic.DataAccess.GetProcesses().Where(p => p.PrcCd.Contains(prePrcCode)).OrderByDescending(p => p.PrcCd).FirstOrDefault();

            if(maxPrc == null)
            {
                resultCode = prePrcCode + "001";
            }
            else
            {
                var maxPrcCd = maxPrc.PrcCd;
                var maxVal = int.Parse(maxPrcCd.Substring(11)) + 1;
                resultCode = prePrcCode + maxVal.ToString("000");
            }
            return resultCode;
        }

        private void StartAnimation()
        {
            //기어 돌아가는 애니메이션
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 360;
            da.Duration = new Duration(TimeSpan.FromSeconds(currSchedule.SchLoadTime)); //일정 계획로드타임
            //da.RepeatBehavior = RepeatBehavior.Forever;

            RotateTransform rt = new RotateTransform();
            Gear1.RenderTransform = rt;
            Gear1.RenderTransformOrigin = new Point(0.5, 0.5);
            Gear2.RenderTransform = rt;
            Gear2.RenderTransformOrigin = new Point(0.5, 0.5);

            rt.BeginAnimation(RotateTransform.AngleProperty, da);

            //제품 애니메이션 속성
            DoubleAnimation ma = new DoubleAnimation();
            ma.From = 120;
            ma.To = 550; // 옮겨지는 x값의 최대값
            ma.Duration = TimeSpan.FromSeconds(currSchedule.SchLoadTime);
            //ma.AccelerationRatio = 0.5;
            //ma.AutoReverse = true;
            Product.BeginAnimation(Canvas.LeftProperty, ma);
        }

        private void BtnStartProcess_Click(object sender, RoutedEventArgs e)
        {
            if (InsertProcessData())
                StartAnimation();
        }
    }
}