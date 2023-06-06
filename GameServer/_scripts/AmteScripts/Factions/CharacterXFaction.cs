using DOL.Database.Attributes;
using DOL.Events;
using DOL.GS;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DOL.Database
{
	[DataTable(TableName = "CharacterXFaction")]
	public class CharacterXFaction : DataObject
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptsCompiled(DOLEvent e, object sender, EventArgs args)
		{
			GameServer.Database.RegisterDataObject(typeof(RvrPlayer));
			log.Info("DATABASE CharacterXFaction LOADED");
		}

		[PrimaryKey]
		public string PlayerID { get; set; }

		[DataElement(AllowDbNull = false)]
		public double FactionValue { get; set; } = 1.0;

		public void Save()
		{
			Dirty = true;
			if (!IsPersisted)
				GameServer.Database.AddObject(this);
			else
				GameServer.Database.SaveObject(this);
		}
	}
}
