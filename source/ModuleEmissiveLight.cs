//
//  ModuleEmissiveLight.cs
//
//  Author: Bonus Eventus 
//
//  Copyright (c) 2018 Bonus Eventus
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
//
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilis
{
	public class ModuleEmissiveLight : PartModule, IScalarModule
	{
		[KSPEvent (guiActive = true, guiName = "Lights Off", guiActiveEditor = true)]
		public void LightsOff ()
		{
			SetLightState (false);
			int count = base.part.symmetryCounterparts.Count;
			while (count-- > 0)
			{
				if (part.symmetryCounterparts [count] != part)
				{
					part.symmetryCounterparts [count].Modules.GetModule<ModuleLight> (0).SetLightState (false);
				}
			}
		}
		
		[KSPEvent (guiActive = true, guiName = "Lights On", guiActiveEditor = true)]
		public void LightsOn ()
		{
			SetLightState (true);
			int count = part.symmetryCounterparts.Count;
			while (count-- > 0)
			{
				if (part.symmetryCounterparts [count] != part)
				{
					part.symmetryCounterparts [count].Modules.GetModule<ModuleLight> (0).SetLightState (true);
				}
			}
		}

		[KSPAction ("Turn Light Off")]
		public void LightOffAction (KSPActionParam param)
		{
			SetLightState (false);
		}
		
		[KSPAction ("Turn Light On")]
		public void LightOnAction (KSPActionParam param)
		{
			if (!this.uiWriteLock)
			{
				SetLightState (true);
			}
		}

		[KSPField(isPersistant=true, guiName="Status", guiActive=true, guiActiveEditor=true)]
		public string statusStr = "Nominal";

		[KSPField]
		public string OKStr = "Nominal";

		[KSPField]
		public string OffStr = "Off";

		[KSPField]
		public string moduleID = "ModuleEmissiveLight";

		//all lights that are intended to be used should have the same name
		[KSPField]
		public string lightName;

		//the speed the light should dim at
		[KSPField]
		public float lightDimSpeed = 1f;

		[KSPField (isPersistant = true)]
		public bool uiWriteLock;

		//should resources be used
		[KSPField]
		public bool useResources;

		//resource consumption rate multiplier
		[KSPField]
		public float mult = 1f;

		[KSPField (isPersistant = true)]
		public bool isOn;

		//each curve represents an RGB channel
		[KSPField]
		public FloatCurve redCurve = new FloatCurve();
		
		[KSPField]
		public FloatCurve greenCurve = new FloatCurve();
		
		[KSPField]
		public FloatCurve blueCurve = new FloatCurve();

		[KSPField]
		public FloatCurve alphaCurve = new FloatCurve();

		private bool isStarted;

		private float resourceFraction;

		private UtilisLightList lightList;

		//IConsumeResources required lists
		
		private List<PartResourceDefinition> consumedResources;

		public List<PartResourceDefinition> GetConsumedResources ()
		{
			return consumedResources;
		}

		public override string GetInfo ()
		{
			string text = string.Empty;
			if (useResources)
			{
				text += resHandler.PrintModuleResources ((double)mult);
			}
			return text;
		}

		private Color lightColor;

		private Color lastColor;

		private float currentTime;

		private float dimRate;

		public bool CanMove
		{
			get
			{
				return !useResources || resourceFraction > 0.5f;
			}
		}
		
		public float GetScalar
		{
			get
			{
				return (!isOn) ? 0f : 1f;
			}
		}
		
		public EventData<float, float> OnMoving
		{
			get
			{
				return OnMove;
			}
		}
		
		public EventData<float> OnStop
		{
			get
			{
				return OnStopped;
			}
		}
		
		private EventData<float, float> OnMove = new EventData<float, float> ("OnMove");
		
		private EventData<float> OnStopped = new EventData<float> ("OnStop");
		
		public string ScalarModuleID
		{
			get
			{
				return moduleID;
			}
		}
		
		//
		// Methods
		//
		public bool IsMoving()
		{
			return false;
		}
		
		public void SetScalar (float t)
		{
			if (t > 0.5f)
			{
				if (!isOn)
				{
					LightsOn ();
				}
			}
			else
			{
				if (t <= 0.5f && isOn)
				{
					LightsOff ();
				}
			}
		}
		
		public void SetUIRead (bool state)
		{
		}
		
		public void SetUIWrite (bool state)
		{
			uiWriteLock = !state;
			if (state)
			{
				Events ["LightsOn"].active = !isOn;
				Events ["LightsOff"].active = isOn;
			}
			else
			{
				Events ["LightsOn"].active = false;
				Events ["LightsOff"].active = false;
			}
		}

		public void SetLightState (bool state)
		{
			if (state)
			{
				isOn = true;
				statusStr = OKStr;
			}
			else
			{
				isOn = false;
				statusStr = OffStr;
			}

			base.Events ["LightsOn"].active = !state;
			base.Events ["LightsOff"].active = state;
		}

		//callback for ModuleColorSelect
		private void GetColor(Color color)
		{
			lightColor = color;
		}

		public override void OnAwake ()
		{
			base.OnAwake ();
			if (this.consumedResources == null)
			{
				this.consumedResources = new List<PartResourceDefinition> ();
			}
			else
			{
				this.consumedResources.Clear ();
			}
			int i = 0;
			int count = this.resHandler.inputResources.Count;
			while (i < count)
			{
				this.consumedResources.Add (PartResourceLibrary.Instance.GetDefinition (this.resHandler.inputResources [i].name));
				i++;
			}
		}

		public override void OnStart (PartModule.StartState state)
		{
			List<Light> lights = new List<Light> (part.FindModelComponents<Light> (lightName));

			lightList = new UtilisLightList(lights);

			if (this.isOn)
			{
				LightsOn ();
			}
			else
			{
				LightsOff ();
			}
			if (HighLogic.LoadedSceneIsFlight)
			{
				isStarted = true;
				if (vessel.situation == Vessel.Situations.PRELAUNCH && isOn)
				{
					vessel.ActionGroups [KSPActionGroup.Light] = true;
				}
			}
		}

		public void FixedUpdate ()
		{
			if (HighLogic.LoadedSceneIsEditor)
			{
				if (lastColor != lightColor)
				{

				}
				lastColor = lightColor;
				return;
			}

			if (isStarted)
			{
				float fixedDeltaTime = TimeWarp.fixedDeltaTime;
				if (isOn && useResources)
				{
					resourceFraction = (float)this.resHandler.UpdateModuleResourceInputs (ref this.statusStr, 1.0, 0.99, false, false, true);
					if (resourceFraction >= 0.99f)
					{
						if (statusStr != OKStr)
						{
							statusStr = OKStr;
						}
					}
					else
					{
						SetLightState (false);
					}

					if (currentTime < 1f)
					{
						dimRate += lightDimSpeed * fixedDeltaTime;
						currentTime = Mathf.Lerp (0f, 1f, dimRate);
						Color c = new Color();
						c.r = redCurve.Evaluate(currentTime);
						c.g = greenCurve.Evaluate(currentTime);
						c.b = blueCurve.Evaluate(currentTime);
						lightList.SetProp(c);
					}

				}else{
					dimRate -= lightDimSpeed * fixedDeltaTime;
					currentTime = Mathf.Lerp (1f, 0f, dimRate);
					Color c = new Color();
					c.r = redCurve.Evaluate(currentTime);
					c.g = greenCurve.Evaluate(currentTime);
					c.b = blueCurve.Evaluate(currentTime);
					lightList.SetProp(c);
				}
			}
		}

		//end
	}
}