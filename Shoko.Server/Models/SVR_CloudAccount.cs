﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using System.Xml.Serialization;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem;
using Shoko.Models.Server;
using Shoko.Server.Repositories;

namespace Shoko.Server.Models
{
    public class SVR_CloudAccount : CloudAccount
    {
        public SVR_CloudAccount()
        {
        }

        public new string Provider
        {
            get { return base.Provider; }
            set
            {
                base.Provider = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _plugin = CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == value);
                }
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        public byte[] Bitmap => _plugin?.Icon;

        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        public byte[] Icon => _plugin?.Icon;
        private ICloudPlugin _plugin;

        [NotMapped]
        [JsonIgnore]
        [XmlIgnore]
        public IFileSystem FileSystem
        {
            get
            {
                if (!ServerState.Instance.ConnectedFileSystems.ContainsKey(Name))
                {
                    ServerState.Instance.ConnectedFileSystems[Name] = Connect();
                    if (NeedSave)
                        Repo.CloudAccount.BeginUpdate(this).Commit();
                }
                return ServerState.Instance.ConnectedFileSystems[Name];
            }
            set
            {
                if (value != null)
                    ServerState.Instance.ConnectedFileSystems[Name] = value;
                else if (ServerState.Instance.ConnectedFileSystems.ContainsKey(Name))
                    ServerState.Instance.ConnectedFileSystems.Remove(Name);
            }
        }
        [NotMapped]
        public bool IsConnected => ServerState.Instance.ConnectedFileSystems.ContainsKey(Name ?? string.Empty);

        [NotMapped]
        [JsonIgnore]
        [XmlIgnore]
        internal bool NeedSave { get; set; } = false;

        private static AuthorizationFactory _cache; //lazy init, because 
        private static AuthorizationFactory AuthInstance
        {
            get { return new AuthorizationFactory("AppGlue.dll"); }
        }

        public IFileSystem Connect()
        {
            if (string.IsNullOrEmpty(Provider))
                throw new Exception("Empty provider supplied");

            Dictionary<string, object> auth = AuthInstance.AuthorizationProvider.Get(Provider);
            if (auth == null)
                throw new Exception("Application Authorization Not Found");
            _plugin = CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == Provider);
            if (_plugin == null)
                throw new Exception("Cannot find cloud provider '" + Provider + "'");
            FileSystemResult<IFileSystem> res = _plugin.Init(Name, new UI.AuthProvider(), auth, ConnectionString);
            if (res == null || !res.IsOk)
                throw new Exception("Unable to connect to '" + Provider + "'");
            string userauth = res.Result.GetUserAuthorization();
            if (ConnectionString != userauth)
            {
                NeedSave = true;
                ConnectionString = userauth;
            }
            return res.Result;
        }

        public static SVR_CloudAccount CreateLocalFileSystemAccount()
        {
            return new SVR_CloudAccount
            {
                Name = "NA",
                Provider = "Local File System"
            };
        }
    }
}