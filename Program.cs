using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor {
    public class Program {
        static void Main(string[] args) {
            var programArgs = ParseArgs(args);
            if (programArgs.Count == 0) {
                Console.WriteLine("Please specify applications to start and watch. For example:");
                Console.WriteLine("Start and monitor one app:");
                Console.WriteLine("supervisor myapp.exe");
                Console.WriteLine("Start and monitor two apps:"); 
                Console.WriteLine("supervisor app1.exe app2.exe");
                Console.WriteLine("Start and monitor two apps with arguments:");
                Console.WriteLine("supervisor \"app1.exe \"\"argument 1\"\"\" \"\"\"Folder Name\\app1.exe\"\" \"\"argument 1\"\"\"");
                return;
            }

            var threads = new List<MonitorThread>(programArgs.Count);

            for (int i = 0; i < programArgs.Count; i++) {
                var t = new MonitorThread(programArgs[i]);
                threads.Add(t);
            }

            Console.WriteLine("Press enter to stop all processes.");
            Console.ReadLine();
            for (int i = 0; i < threads.Count; i++) {
                threads[i].Stop();
            }
            for (int i = 0; i < threads.Count; i++) {
                threads[i].Join();
            }
        }

        public static List<ProgramArgs> ParseArgs(string[] args) {
            var res = new List<ProgramArgs>(args.Length);
            for (int i = 0; i < args.Length; i++) {
                var arg = args[i];
                if (arg[0] == '\"') {
                    var quoteIndex = arg.IndexOf('\"', 1);
                    res.Add(new ProgramArgs {
                        ExeName = arg.Substring(1, quoteIndex - 1),
                        Arguments = arg.Substring(quoteIndex + 1).Trim()
                    });
                } else {
                    var spaceIndex = arg.IndexOf(' ');
                    res.Add(new ProgramArgs {
                        ExeName = arg.Substring(0, spaceIndex),
                        Arguments = arg.Substring(spaceIndex + 1)
                    });
                }
            }
            return res;
        }

    }

    public class MonitorThread {
        public readonly ProgramArgs Args;
        
        public Process Process {
            get;
            private set;
        }

        private bool IsCancelled;

        private readonly Thread Thread;

        public MonitorThread(ProgramArgs args) {
            Args = args;
            Thread = new Thread(DoMonitor);
            Thread.Start();
        }

        private void DoMonitor() {
            Process = Process.Start(Args.ExeName, Args.Arguments);
            while (true) {
                if (Process.WaitForExit(1000) && !IsCancelled) {
                    Process = Process.Start(Args.ExeName, Args.Arguments);
                }
                if (IsCancelled) {
                    if (!Process.HasExited)
                        Process.Kill();
                    return;
                }
            }
        }

        public void Stop() {
            IsCancelled = true;
        }

        public void Join() {
            if (Thread.IsAlive)
                Thread.Join();
        }

        public bool Join(TimeSpan timeout) {
            if (Thread.IsAlive)
                return Thread.Join(timeout);
            else
                return true;
        }
    }

    public class ProgramArgs {
        public string ExeName;
        public string Arguments;

        public override bool Equals(object obj) {
            if (obj is ProgramArgs) {
                var b = obj as ProgramArgs;
                return ExeName == b.ExeName && Arguments == b.Arguments;
            } else
                return false;
        }

        public override string ToString() {
            return "\"" + ExeName + "\" " + Arguments;
        }

        public override int GetHashCode() {
            var result = ExeName.GetHashCode();
            result = 31 * result + Arguments.GetHashCode();
            return result;
        }
    }
}
