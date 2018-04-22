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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
namespace Utilis
{
	public class ModuleEmissiveLight : PartModule, IResourceConsumer
	{
		//GUI Stuff
		[KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Intensity", advancedTweakable = true), UI_FloatRange (minValue = 0f, maxValue = 3f, stepIncrement = 0.05f)]
		public float Intensity = 1f;

		[KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Range", advancedTweakable = true), UI_FloatRange (minValue = 0f, maxValue = 3f, stepIncrement = 0.05f)]
		public float Range = 1f;

		[KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "SpotAngle", advancedTweakable = true), UI_FloatRange (minValue = 0f, maxValue = 3f, stepIncrement = 0.05f)]
		public float SpotAngle = 1f;

		[KSPEvent(guiName="Turn On", active = true, guiActive=true, guiActiveEditor=true)]
		public void LightOn()
		{
			SetLightState(true);

			int count = part.symmetryCounterparts.Count;

			if(count != 0 || part.symmetryCounterparts != null)
			{
				while(count-- >0)
				{
					Part p = part.symmetryCounterparts[count];

					if(part != p) p.Modules.GetModule<ModuleEmissiveLight> (0).SetLightState(true);
				}
			}
		}

		[KSPEvent(guiName="Turn Off", active = false, guiActive=true, guiActiveEditor=true)]
		public void LightOff()
		{
			SetLightState(false);

			int count = part.symmetryCounterparts.Count;

			if(count != 0 || part.symmetryCounterparts != null)
			{
				while(count-- >0)
				{
					Part p = part.symmetryCounterparts[count];

					if(part != p) p.Modules.GetModule<ModuleEmissiveLight> (0).SetLightState(false);
				}
			}
		}

		[KSPAction("Toggle Light")]
		public void ToggleLight(KSPActionParam param)
		{
			if(isOn)
			{
				LightOff ();
			}
			else
			{
				LightOn ();
			}
		}

		[KSPField(guiName="Status", guiActive=true,guiActiveEditor=true)]
		public string status = "Nominal";


		//Fields

		[KSPField]
		public string ModuleID = "ModuleEmissiveLight";

		[KSPField(isPersistant=true)]
		public bool isOn = false;

		[KSPField(isPersistant=true)]
		public bool operational = true;

		[KSPField]
		public bool AdvancedLightTweakable = false;

		[KSPField]
		public string ShaderProperty = "_EmissiveColor";

		[KSPField]
		public string lightsName;

		[KSPField]
		public string excludeRenderList;

		//don't override
		[KSPField (isPersistant=true)]
		public Vector3 _c = new Vector3(1f,1f,1f);

		[KSPField]
		public FloatCurve redCurve = new FloatCurve();

		[KSPField]
		public FloatCurve greenCurve = new FloatCurve();

		[KSPField]
		public FloatCurve blueCurve = new FloatCurve();

		[KSPField (isPersistant=true)]
		public float curveTime = 0f;

		[KSPField (isPersistant=true)]
		public bool Animate = true;

		[KSPField]
		public float Efficiency = 1f;

		[KSPField]
		public float DimSpeed = 1f;

		[KSPField]
		public bool DrawRsources = false;

		private List<PartResourceDefinition> consumedResources;

		//needed by IResourceConsumer interface
		public List<PartResourceDefinition> GetConsumedResources ()
		{
			return consumedResources;
		}

		[KSPField]
		public float TimeRate = 0.1f;

		private UtilisLightList LightList;

		private UtilisRendererList renderers;

		public List<Renderer> excludedRenderers;

		private BaseEvent turnOn;

		private BaseEvent turnOff;

		private float dtCounter;

		private void hasResources()
		{
			if (!resHandler.UpdateModuleResourceInputs (ref status, Efficiency, 0.9, true, true))
			{
				operational = false;
			}
			else if(!operational)
			{
				operational = true;
			}
		}

		public Vector3 Prop
		{
			get
			{
				return new Vector3 (Intensity,Range,SpotAngle);
			}
			set
			{
				Intensity = value.x;
				Range = value.y;
				SpotAngle = value.z;
			}
		}

		public Color Color
		{
			get
			{
				//Color is returns the value of the FloatCurves evaluated multiplied by the stored color '_c'
				//_c is stored as a persistent Vector3
				Color c = new Color();

				c.r = _c.x * redCurve.Evaluate(curveTime);

				c.g = _c.y * greenCurve.Evaluate(curveTime);

				c.b = _c.z * blueCurve.Evaluate(curveTime);

				c.a = 1f;

				return c;
			}

			set
			{
				_c.x = value.r;

				_c.y = value.g;

				_c.z = value.b;
			}
		}

		//Methods

		public void SetLightState(bool state)
		{
			if(state)
			{
				isOn = true;

				Animate = true;

				LightList.Enable();

				turnOn.active = false;

				turnOff.active = true;
			}
			else
			{
				isOn = false;

				turnOn.active = true;

				turnOff.active = false;
			}
		}

		public void UpdateColor(Color color, bool ignoreColor)
		{
			if(!ignoreColor)this.Color = color;

			if(LightList.State)
			{
				LightList.SetProp(this.Color);

				renderers.SetColor(this.Color);
			}
		}

		private void setupFields(BaseField field, bool state)
		{
			field.guiActiveEditor = state;
			UI_FloatRange range = (UI_FloatRange)field.uiControlEditor;
			range.affectSymCounterparts = UI_Scene.All;
			range.onFieldChanged = UpdateLightProp;
			Debug.Log ("[ModuleColorSelect] "+field.name+" setup complete");
		}

		public void UpdateLightProp(BaseField field, object oldValueObj)
		{
			SetProp (Prop);

			int count = part.symmetryCounterparts.Count;

			if(count > 0 || part.symmetryCounterparts != null)
			{
				while(count-- >0)
				{
					Part p = part.symmetryCounterparts[count];

					if(part != p) p.Modules.GetModule<ModuleEmissiveLight> (0).SetProp(Prop);
				}
			}
		}

		public void SetProp(Vector3 prop)
		{
			Prop = prop;
			LightList.SetProp (prop);
		}

		public void Update()
		{
			if(HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) 
			{
				if(operational && isOn || Animate)UpdateColor (Color.black, true);
			}
		}

		public void FixedUpdate()
		{
			if(operational && isOn)
			{
				if(DrawRsources)hasResources ();

				if(curveTime < 1f)
				{
					curveTime = Mathf.Lerp(0f,1f,dtCounter);

					dtCounter += TimeRate * DimSpeed;
				}
			}
			else if(Animate)
			{
				if(curveTime > 0f)
				{
					curveTime = Mathf.Lerp(0f,1f,dtCounter);

					dtCounter -= TimeRate * DimSpeed;

				}
				else if(curveTime <= 0f) 
				{
					LightList.Disable();

					Animate = false;
				}

			}
			else if(!operational)
			{
				if(DrawRsources)hasResources ();
			}
		}

		private void Setup()
		{
			//get events
			//u = new UtilisLog("["+ModuleID+"]");

			turnOn = base.Events["LightOn"];

			turnOff = base.Events["LightOff"];

			setupFields(base.Fields["Intensity"], AdvancedLightTweakable);

			setupFields(base.Fields["Range"] , AdvancedLightTweakable);

			setupFields(base.Fields["SpotAngle"], AdvancedLightTweakable);

			//u.Log ("base.Events setup complete.");

			//u.Log ("ToggleState called.");

			//get lights
			if(lightsName != string.Empty)
			{
				List<Light> lights = part.FindModelComponents<Light>(lightsName);

				if(lights.Count != 0 || lights != null)
				{
					LightList = new UtilisLightList(lights);

					//u.Log ("LightList created");
				}
				else
				{
					//u.Exception("<string> lightsName FindModelComponents query returned null or had a length of 0.");
				}
			}
			else
			{
				//u.Exception("<string> lightsName is empty.");
			}

			//get renderers
			List<Renderer> rList = part.FindModelComponents<Renderer>();

			if(!string.IsNullOrEmpty(excludeRenderList))
			{
				if(rList != null || rList.Count >0)
				{
					renderers =  new UtilisRendererList(rList,excludedRenderers, ShaderProperty);
				}
				else
				{
					//u.Exception("No Unity Renderer components found in model.mu file.");
				}
			}
			else
			{
				//u.Log ("No excluded Renderers found.");
				if(rList != null || rList.Count >0)renderers =  new UtilisRendererList(rList, ShaderProperty);
			}

			//u.Log ("LightList state is set.");

			//u.Log ("renderers state is set.");

			//set events state
			if(isOn)
			{
				LightList.Enable();
				//UpdateColor(Color.cyan);
				turnOn.active = false;

				turnOff.active = true;
			}
			else
			{
				LightList.Disable();
				//UpdateColor(Color.black);
				turnOn.active = true;

				turnOff.active = false;
			}

			//part.AddOnMouseExit(new Part.OnActionDelegate(OnMouseExit));
		}

		public override void OnLoad (ConfigNode node)
		{
			base.OnLoad (node);
			//create new log object
			//u = new UtilisLog("["+ModuleID+"]");

			string list = string.Empty;

			if(node.HasValue("excludeRenderList"))
			{
				Debug.Log ("node.HasValue(excludeRenderList");

				list = node.GetValue("excludeRenderList").ToString();

				list = Regex.Replace(list, @"\s+", string.Empty);

				string[] array = list.Split(new char[]{';'});

				Debug.Log ("string sanitized.");

				if(array != null || array.Length >0)
				{
					Debug.Log ("array was not null and it was longer than 0");

					excludedRenderers = new List<Renderer>();

					int count = array.Length;

					while(count-- >0)
					{
						Renderer r = part.FindModelComponent<Renderer>(array[count]);

						if(r != null)excludedRenderers.Add(r);
					}
				}
			}
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

		public override void OnStart (StartState state)
		{
			Debug.Log ("onStart has begun executing.");

			base.OnStart (state);

			Setup();
		}

		public override void OnStartFinished (StartState state)
		{
			base.OnStartFinished (state);
			ModuleSelectColor msc = part.FindModulesImplementing<ModuleSelectColor>()[0];
			//ModuleTweakable mt = part.FindModulesImplementing<ModuleTweakable>()[0];
			if(msc != null)msc.Add(UpdateColor);
			//if(manualDeploy)if(mt != null)mt.Add(GetValue);
		}
	}
}




























