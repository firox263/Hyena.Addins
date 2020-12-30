//
// DatabaseConfiguration.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Hyena.Addins.Database
{
	internal class DatabaseConfiguration
	{
		Dictionary<string,AddinStatus> addinStatus = new Dictionary<string,AddinStatus> ();

		internal class AddinStatus
		{
			public AddinStatus (string addinId)
			{
				this.AddinId = addinId;
			}

			public string AddinId;
			public bool ConfigEnabled;
			public bool? SessionEnabled;
			public bool Uninstalled;
			public List<string> Files;
			public bool Enabled {
				get { return SessionEnabled ?? ConfigEnabled; }
			}
		}

		public bool IsEnabled (string addinId, bool defaultValue)
		{
			var addinName = Addin.GetIdName (addinId);

			AddinStatus s;

			// If the add-in is globaly disabled, it is disabled no matter what the version specific status is
			if (addinStatus.TryGetValue (addinName, out s)) {
				if (!s.Enabled)
					return false;
			}

			if (addinStatus.TryGetValue (addinId, out s))
				return s.Enabled && !IsRegisteredForUninstall (addinId);
			else
				return defaultValue;
		}
		
		public void SetEnabled (string addinId, bool enabled, bool defaultValue, bool exactVersionMatch, bool onlyForTheSession = false)
		{
			if (IsRegisteredForUninstall (addinId))
				return;

			var addinName = exactVersionMatch ? addinId : Addin.GetIdName (addinId);

			AddinStatus s;
			addinStatus.TryGetValue (addinName, out s);
			
			if (s == null)
				s = addinStatus [addinName] = new AddinStatus (addinName);
			if (onlyForTheSession)
				s.SessionEnabled = enabled;
			else {
				s.ConfigEnabled = enabled;
				s.SessionEnabled = null;
			}

			// If enabling a specific version of an add-in, make sure the add-in is enabled as a whole
			if (enabled && exactVersionMatch)
				SetEnabled (addinId, true, defaultValue, false, onlyForTheSession);
		}
		
		public void RegisterForUninstall (string addinId, IEnumerable<string> files)
		{
			AddinStatus s;
			if (!addinStatus.TryGetValue (addinId, out s))
				s = addinStatus [addinId] = new AddinStatus (addinId);
			
			s.ConfigEnabled = false;
			s.Uninstalled = true;
			s.Files = new List<string> (files);
		}
		
		public void UnregisterForUninstall (string addinId)
		{
			addinStatus.Remove (addinId);
		}
		
		public bool IsRegisteredForUninstall (string addinId)
		{
			AddinStatus s;
			if (addinStatus.TryGetValue (addinId, out s))
				return s.Uninstalled;
			else
				return false;
		}
		
		public bool HasPendingUninstalls {
			get { return addinStatus.Values.Where (s => s.Uninstalled).Any (); }
		}
		
		public AddinStatus[] GetPendingUninstalls ()
		{
			return addinStatus.Values.Where (s => s.Uninstalled).ToArray ();
		}
		
		public static DatabaseConfiguration Read (string file)
		{
			var config = ReadInternal (file);
			// Try to read application level config to support disabling add-ins by default.
			var appConfig = ReadAppConfig ();

			if (appConfig == null)
				return config;

			// Overwrite app config values with user config values
			foreach (var entry in config.addinStatus)
				appConfig.addinStatus [entry.Key] = entry.Value;

			return appConfig;
		}
		
		public static DatabaseConfiguration ReadAppConfig()
		{
			var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			var assemblyDirectory = Path.GetDirectoryName (assemblyPath);
			var appAddinsConfigFilePath = Path.Combine (assemblyDirectory, "addins-config.xml");

			if (!File.Exists (appAddinsConfigFilePath))
				return new DatabaseConfiguration ();

			return ReadInternal (appAddinsConfigFilePath);
		}

		static DatabaseConfiguration ReadInternal (string file)
		{
			DatabaseConfiguration config = new DatabaseConfiguration ();
			XmlDocument doc = new XmlDocument ();
			doc.Load (file);
			
			XmlElement disabledElem = (XmlElement) doc.DocumentElement.SelectSingleNode ("DisabledAddins");
			if (disabledElem != null) {
				// For back compatibility
				foreach (XmlElement elem in disabledElem.SelectNodes ("Addin"))
					config.SetEnabled (elem.InnerText, false, true, false);
				return config;
			}
			
			XmlElement statusElem = (XmlElement) doc.DocumentElement.SelectSingleNode ("AddinStatus");
			if (statusElem != null) {
				foreach (XmlElement elem in statusElem.SelectNodes ("Addin")) {
					AddinStatus status = new AddinStatus (elem.GetAttribute ("id"));
					string senabled = elem.GetAttribute ("enabled");
					status.ConfigEnabled = senabled.Length == 0 || senabled == "True";
					status.Uninstalled = elem.GetAttribute ("uninstalled") == "True";
					config.addinStatus [status.AddinId] = status;
					foreach (XmlElement fileElem in elem.SelectNodes ("File")) {
						if (status.Files == null)
							status.Files = new List<string> ();
						status.Files.Add (fileElem.InnerText);
					}
				}
			}
			return config;
		}
		
		public void Write (string file)
		{
			StreamWriter s = new StreamWriter (file);
			using (s) {
				XmlTextWriter tw = new XmlTextWriter (s);
				tw.Formatting = Formatting.Indented;
				tw.WriteStartElement ("Configuration");
				
				tw.WriteStartElement ("AddinStatus");
				foreach (AddinStatus e in addinStatus.Values) {
					tw.WriteStartElement ("Addin");
					tw.WriteAttributeString ("id", e.AddinId);
					tw.WriteAttributeString ("enabled", e.ConfigEnabled.ToString ());
					if (e.Uninstalled)
						tw.WriteAttributeString ("uninstalled", "True");
					if (e.Files != null && e.Files.Count > 0) {
						foreach (var f in e.Files)
							tw.WriteElementString ("File", f);
					}
					tw.WriteEndElement ();
				}
				tw.WriteEndElement (); // AddinStatus
				tw.WriteEndElement (); // Configuration
			}
		}
	}
}
