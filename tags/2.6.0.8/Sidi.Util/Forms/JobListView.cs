using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using Sidi.Extensions;
using Sidi.ComponentModel;

namespace Sidi.Forms
{
    public class JobListView : ListView
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JobListView()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.View = System.Windows.Forms.View.Details;
            this.Columns.Add("State", 100);
            this.Columns.Add("Name", 200);
            this.Columns.Add("Progress", 50);
            this.Columns.Add("State", -2);
            this.FullRowSelect = true;
            this.MultiSelect = true;

            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateItems();
        }

        System.Windows.Forms.Timer timer = new Timer();

        void UpdateList()
        {
            this.Items.Clear();
            if (JobList == null)
            {
                return;
            }

            this.Items.AddRange(
                JobList.Jobs.Select(job =>
                    {
                        var lvi = new ListViewItem()
                        {
                            Tag = job,
                        };
                        lvi.SubItems.Add(String.Empty);
                        lvi.SubItems.Add(String.Empty);
                        lvi.SubItems.Add(String.Empty);
                        return lvi;
                    })
                .ToArray());
        }

        bool listChanged = false;

        void UpdateItems()
        {
            try
            {
                BeginUpdate();

                if (listChanged)
                {
                    UpdateList();
                    listChanged = false;
                }

                foreach (ListViewItem lvi in this.Items)
                {
                    var job = (Job)lvi.Tag;
                    lvi.Text = job.State.ToString();
                    int i = 0;
                    lvi.SubItems[++i].Text = job.Name;
                    lvi.SubItems[++i].Text = job.LastProgressChangedEventArgs.ProgressPercentage.ToString();
                    lvi.SubItems[++i].Text = job.LastProgressChangedEventArgs.UserState.SafeToString();
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        JobList jobList;
        public JobList JobList
        {
            get
            {
                if (jobList == null)
                {
                    JobList = new JobList();
                }
                return jobList;
            }

            set
            {
                if (jobList != null)
                {
                    jobList.Jobs.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Jobs_CollectionChanged);
                }
                jobList = value;
                listChanged = true;
                jobList.Jobs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Jobs_CollectionChanged);
            }
        }

        void Jobs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            listChanged = true;
        }
    }
}
