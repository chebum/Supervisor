using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Supervisor.Tests {
    [TestClass]
    public class SupervisorTests {
        [TestMethod]
        public void ParseArgs() {
            var args = Program.ParseArgs(new string[] {
                "test.exe 1 \"88\"",
                "\"C:\\Program Files\\test.exe\" \"33\""
            });
            Assert.AreEqual(2, args.Count);
            Assert.AreEqual(new ProgramArgs {
                ExeName = "test.exe",
                Arguments = "1 \"88\""
            }, args[0]);
            Assert.AreEqual(new ProgramArgs {
                ExeName = "C:\\Program Files\\test.exe",
                Arguments = "\"33\""
            }, args[1]);
        }

        [TestMethod]
        public void MonitorProcess() {
            var args = new ProgramArgs {
                ExeName = "cmd.exe"
            };
            var monitor = new MonitorThread(args);
            try {
                // Process won't start until this thread is idle
                var waitForProcess = new Thread(delegate () {
                    for (int i = 0; i < 30; i++) {
                        if (monitor.Process != null)
                            break;
                        else
                            Thread.Sleep((i + 1) * 100);
                    }
                });
                waitForProcess.Start();
                waitForProcess.Join();
                Console.WriteLine("test");
                Assert.IsNotNull(monitor.Process);
                Assert.IsFalse(monitor.Process.HasExited);
                
                var process1 = monitor.Process;
                monitor.Process.Kill();
                waitForProcess = new Thread(delegate () {
                    for (int i = 0; i < 30; i++) {
                        if (monitor.Process != process1)
                            break;
                        else
                            Thread.Sleep((i + 1) * 100);
                    }
                });
                waitForProcess.Start();
                waitForProcess.Join();
                Assert.IsNotNull(monitor.Process);
                
                monitor.Stop();
                monitor.Join();
                Assert.IsTrue(monitor.Process.HasExited);
            } finally {
                monitor.Stop();
                monitor.Join();
            }
        }
    }
}
