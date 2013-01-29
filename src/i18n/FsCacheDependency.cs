using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace i18n
{
    /// <summary>
    /// Custom cache dependency implementation for monitoring file system changes (over and above
    /// what is provided by the standard CacheDependency class).
    /// </summary>
    /// <remarks>
    /// While the standard CacheDependency class supports monitoring of individual files (and possibly folders, not sure),
    /// it doesn't seem to support deep folder (tree) monitoring or provide any other configurability
    /// such as types of FS events to monitor etc.. This class is intended to provide all that.
    /// </remarks>
    public class FsCacheDependency : CacheDependency
    {
    // Data
        protected FileSystemWatcher m_fswatcher;
    // Con
        /// <summary>
        /// Initializes a new instance of the i18n.FsCacheDependency class, given
        /// the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">
        /// The directory to monitor, in standard or Universal Naming Convention (UNC) notation.
        /// </param>
        /// <param name="includeSubdirectories">
        /// Value indicating whether subdirectories within the specified path should be monitored.
        /// Defaults to true.
        /// </param>
        /// <param name="filespec">
        /// The type of files to watch. For example, "*.txt" watches for changes to all text files. Defaults to "*.*".
        /// </param>
        /// <param name="changeTypes">
        /// The type of changes to watch for.
        /// Defaults to a combination of LastWrite, FileName, and DirectoryName.
        /// </param>
        /// <param name="autoStart">
        /// Indicates whether the monitoring it to begin immediately.
        /// If false, the caller must manipulate the EnableRaisingEvents property.
        /// Defaults to true.
        /// </param>
        public FsCacheDependency(
            string path, 
            bool includeSubdirectories = true,
            string filespec = "*.*",
            NotifyFilters changeTypes = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            bool autoStart = true)
        {
           // Init.
            m_fswatcher = new FileSystemWatcher(path, filespec);
            m_fswatcher.IncludeSubdirectories = includeSubdirectories;
            m_fswatcher.NotifyFilter = changeTypes;
           // Wire up event handlers.
            var handler = new FileSystemEventHandler(OnFsEvent);
            m_fswatcher.Changed += handler;
            m_fswatcher.Created += handler;
            m_fswatcher.Deleted += handler;
            m_fswatcher.Renamed += new RenamedEventHandler(OnFsEvent);
           // Conditionally start watching now.
            if (autoStart) {
                m_fswatcher.EnableRaisingEvents = true; }
        }
    // Operations
        /// <summary>
        /// Gets or sets a value indicating whether the component is enabled.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get { return m_fswatcher.EnableRaisingEvents; }
            set { m_fswatcher.EnableRaisingEvents = value; }
        }
    // Events
        protected void OnFsEvent(object sender, FileSystemEventArgs e)
        {
            DebugHelpers.WriteLine("i18n.FsCacheDependency.OnFsEvent -- e:{0}", e);
            this.NotifyDependencyChanged(this, e);
        }
        protected void OnFsEvent(object sender, RenamedEventArgs e)
        {
            DebugHelpers.WriteLine("i18n.FsCacheDependency.OnFsEvent -- e:{0}", e);
            this.NotifyDependencyChanged(this, e);
        }
    // [CacheDependency]
        protected override void DependencyDispose() 
        {
            if (m_fswatcher != null)
            {
                m_fswatcher.Dispose();
                m_fswatcher = null;
            }
            base.DependencyDispose();
        }

    }
}
