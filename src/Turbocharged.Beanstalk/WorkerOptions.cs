﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turbocharged.Beanstalk
{
    public class WorkerOptions
    {
        /// <summary>
        /// The tubes this worker watches. If you do not set any, "default" will be watched automatically.
        /// </summary>
        public List<string> Tubes { get; set; }

        /// <summary>
        /// The action that should be taken if a worker function
        /// throws an unhandled exception.
        /// </summary>
        public WorkerFailureBehavior FailureBehavior { get; set; }

        /// <summary>
        /// The priority to set when burying or releasing a job
        /// which resulted in an unhandled exception.
        /// </summary>
        public int? FailurePriority { get; set; }

        /// <summary>
        /// The delay to set when releasing a job which resulted
        /// in an unhandled exception.
        /// </summary>
        public TimeSpan FailureReleaseDelay { get; set; }

        public WorkerOptions()
        {
            Tubes = new List<string>();
            FailureBehavior = WorkerFailureBehavior.Bury;
            FailurePriority = null;
            FailureReleaseDelay = TimeSpan.Zero;
        }
    }

    public enum WorkerFailureBehavior
    {
        Bury,
        Release,
        Delete,
        NoAction,
    }
}
