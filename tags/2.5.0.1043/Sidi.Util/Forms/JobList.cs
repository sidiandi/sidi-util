using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.ComponentModel;
using System.Collections.ObjectModel;

namespace Sidi.Forms
{
    public class JobList
    {
        public JobList()
        {
            MaxBusyCount = 1;
            Jobs = new ObservableCollection<Job>();
            Jobs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Jobs_CollectionChanged);
        }

        void Jobs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var j in e.NewItems.Cast<Job>())
                {
                    j.BackgroundWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                }
            }
            UpdateRunState();
        }

        public ObservableCollection<Job> Jobs { get; private set; }

        public int MaxBusyCount { get; set; }

        void UpdateRunState()
        {
            var busyCount = Jobs.Count(x => x.State == Job.JobState.Running);
            foreach (var i in Jobs)
            {
                if (busyCount >= MaxBusyCount)
                {
                    break;
                }
                if (i.State == Job.JobState.Pending)
                {
                    i.Run();
                    ++busyCount;
                }
            }
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateRunState();
        }
    }
}
