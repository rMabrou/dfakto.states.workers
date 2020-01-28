using System;
using System.Collections.Generic;
using dFakto.States.Workers.Interfaces;
using Microsoft.Extensions.Configuration;

namespace dFakto.States.Workers.FileStores
{
    public class FileStoreFactoryConfig
    {
        public FileStoreFactoryConfig()
        {
            StoreBuilders = new Dictionary<string, Func<IServiceProvider, string, IConfigurationSection, IFileStore>>();
        }
        
        public FileStoreConfig[] Stores { get; set; }
        
        public Dictionary<string, Func<IServiceProvider, string, IConfigurationSection, IFileStore>> StoreBuilders
        {
            get;
            private set;
        }
    }
}