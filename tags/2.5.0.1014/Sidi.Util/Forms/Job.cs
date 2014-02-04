using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.ComponentModel;

namespace Sidi.Forms
{
    public class Job
    {
        public enum JobState
        {
            Pending,
            Running,
            Completed
        };

        public void Run()
        {
            if (State == JobState.Pending)
            {
                BackgroundWorker.RunWorkerAsync();
            }
        }

        public Job(string name, System.Action action)
        : this(name, CreateBackgroundWorker(action))
        {
        }

        static BackgroundWorker CreateBackgroundWorker(System.Action action)
        {
            return CreateBackgroundWorker(bgw =>
            {
                using (new BackgroundWorkerReportProgressAppender(bgw))
                {
                    action();
                }
            });
        }

        public Job(string name, Action<BackgroundWorker> job)
            : this(name, CreateBackgroundWorker(job))
        {
        }

        static BackgroundWorker CreateBackgroundWorker(Action<BackgroundWorker> job)
        {
            var b = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            b.DoWork += (s, e) =>
            {
                job(b);
                b.ReportProgress(100);
            };

            return b;
        }
        
        public Job(string name, BackgroundWorker worker)
        {
            this.BackgroundWorker = worker;
            this.Name = name;

            BackgroundWorker.ProgressChanged += (s, e) =>
            {
                LastProgressChangedEventArgs = e;
            };

            BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
        }

        public JobState State
        {
            get
            {
                if (completed)
                {
                    return JobState.Completed;
                }
                return BackgroundWorker.IsBusy ? JobState.Running : JobState.Pending;
            }
        }

        bool completed = false;

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            completed = true;
        }

        public string Name;
        public BackgroundWorker BackgroundWorker;
        public ProgressChangedEventArgs LastProgressChangedEventArgs = new ProgressChangedEventArgs(0, null);
    }
}
