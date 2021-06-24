using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRPApp.View.Setting;
using System;
using System.Linq;

namespace MRPApp.Test
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void IsDuplicateDataTest()
        {
            var expectVal = true; //예상 값
            var inputCode = "PC010001";

            var code = Logic.DataAccess.GetSettings().Where(d => d.BasicCode.Contains(inputCode)).FirstOrDefault();
            var realVal = code != null ? true : false;//진짜 값

            Assert.AreEqual(expectVal, realVal); // 값이 같으면 Pass, 다르면 Fail
        }
        [TestMethod]
        public void IsCodeSearched()
        {
            var expectVal = 2; //예상 값
            var inputCode = "설비";

            var realVal = Logic.DataAccess.GetSettings().Where(d => d.CodeName.Contains(inputCode)).Count();

            Assert.AreEqual(expectVal, realVal); // 값이 같으면 Pass, 다르면 Fail
        }
        [TestMethod]
        public void IsEmailCorrect()
        {
            var inputEmail = "guemin12@naver.com";
            Assert.IsTrue(Commons.IsValidEmail(inputEmail));
        }
        [TestMethod]
        public void IsEmailInCorrect()
        {
            var inputEmail = "guemin12@naver";
            Assert.IsFalse(Commons.IsValidEmail(inputEmail));
        }
    }
}
