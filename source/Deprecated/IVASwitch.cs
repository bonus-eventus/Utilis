//
//  IVASwitch.cs
//
//  Author:
//       Bonus Eventus <>
//
//  Copyright (c) 2016 Bonus Eventus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using System.IO;
namespace Utilis
{
	public class IVASwitch : PartModule
	{
		public IVASwitch ()
		{
		}
		[KSPField]
		public string internalModelName;

		[KSPField]
		public string internalConfigPath;

		[KSPField]
		public string orgInternalConfigPath;

		[KSPField(isPersistant=true)]
		public string orgInternalModelName;

		[KSPField(isPersistant=true, guiName="IVA Name", guiActiveEditor=true)]
		public string currentIVA;

		[KSPEvent(guiName="Switch IVA", guiActiveEditor=true)]
		public void switchIVA()
		{
			if(!ivaToggle)
			{
				//toggle true
				part.InternalModelName = internalModelName;
				currentIVA = internalModelName;
				switchConfig(internalConfigPath);
			}
			else 
			{
				//toggle false
				part.InternalModelName = orgInternalModelName;
				currentIVA = orgInternalModelName;
				switchConfig(orgInternalConfigPath);
			}
			ivaToggle = !ivaToggle;
		}

		//[KSPField]
		public bool ivaToggle = false;

		public void switchConfig(string path)
		{
			part.partInfo.internalConfig = ConfigNode.Load(path);
		}

		public override void OnStart (StartState state)
		{
			base.OnStart (state);
			if(part.internalModel)
			{
				int count = this.part.protoModuleCrew.Count;
				for (int i = 0; i < count; i++)
				{
					//KerbalAnimationManager km = new KerbalAnimationManager();

					//part.protoModuleCrew[i].
					i++;
				}
			}
			if( part.internalModel)
			orgInternalModelName = part.InternalModelName;
			currentIVA = part.InternalModelName;
		}
	}
}

