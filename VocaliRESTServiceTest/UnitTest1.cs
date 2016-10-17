using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VocaliRESTServiceTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodAdd()
        {
            VocaliRESTService.TranscripcionService service = new VocaliRESTService.TranscripcionService();
            service.AddTranscripcion("eva", "audio-eva-test-001.mp3", new byte[5000]);
        }
        [TestMethod]
        public void TestMethodSelectCU2a()
        {
            VocaliRESTService.TranscripcionService service = new VocaliRESTService.TranscripcionService();
            service.GetTranscripcionListByLogin("rafael");
        }
        [TestMethod]
        public void TestMethodSelectCU2b()
        {
            VocaliRESTService.TranscripcionService service = new VocaliRESTService.TranscripcionService();
            service.GetTranscripcionListByLoginFechaRecepcion("gabriel", "201609010000-");
        }
        [TestMethod]
        public void TestMethodSelectCU3()
        {
            VocaliRESTService.TranscripcionService service = new VocaliRESTService.TranscripcionService();
            service.GetTranscripcionById("5");
        }
    }
}
