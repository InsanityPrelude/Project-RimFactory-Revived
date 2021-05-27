﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using ProjectRimFactory.AutoMachineTool;
using RimWorld.Planet;


namespace ProjectRimFactory.Common
{
    static class CommonGUIFunctions
    {

		//Adaption of "Verse.Widgets.Label(Rect rect, string label)" To expose GUIStyle
		//This enabel the control over the Text Style
		public static void Label(Rect rect, string label,GUIStyle gUIStyle)
		{

			Rect val = rect;
			float num = Prefs.UIScale / 2f;
			if (Prefs.UIScale > 1f && Math.Abs(num - Mathf.Floor(num)) > float.Epsilon)
			{
				val.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
				val.xMax = Widgets.AdjustCoordToUIScalingFloor(rect.xMax);
				val.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
				val.yMax = Widgets.AdjustCoordToUIScalingFloor(rect.yMax);
			}
			GUI.Label(val, label, gUIStyle);
		}



		//Adaption of Verse.Widgets.ThingIcon(Rect rect, Thing thing, float alpha = 1f)
		//With the intend to cache the Graphic

		//rect for the size in case of a corpse
		//thing for the refrence
		public static Texture GetThingTextue(Rect rect, Thing thing, out Color color)
        {
			color = thing.DrawColor;
			thing = thing.GetInnerIfMinified();
			if (!thing.def.uiIconPath.NullOrEmpty())
			{
				return (Texture)(object)thing.def.uiIcon;
			}
			else if (thing is Pawn || thing is Corpse)
			{
				Pawn pawn = thing as Pawn;
				if (pawn == null)
				{
					pawn = ((Corpse)thing).InnerPawn;
				}
				if (!pawn.RaceProps.Humanlike)
				{
					if (!pawn.Drawer.renderer.graphics.AllResolved)
					{
						pawn.Drawer.renderer.graphics.ResolveAllGraphics();
					}
					Material obj = pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(Rot4.East);
					color = obj.color;
					return obj.mainTexture;

				}
				else
				{
					rect = rect.ScaledBy(1.8f);
					rect.y += 3f;
					rect = rect.Rounded();
					return (Texture)(object)PortraitsCache.Get(pawn, new Vector2(((Rect)(rect)).width, ((Rect)(rect)).height));
				}
			}
			else
			{
				return thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot).mainTexture;
			}
		}



		public static void ThingIcon(Rect rect, Thing thing, Texture resolvedIcon , Color color, float alpha = 1f)
		{
			thing = thing.GetInnerIfMinified();
			GUI.color = color;
			float resolvedIconAngle = 0f;
			if (!thing.def.uiIconPath.NullOrEmpty())
			{
				resolvedIconAngle = thing.def.uiIconAngle;
				rect.position = rect.position + new Vector2(thing.def.uiIconOffset.x * ((Rect)(rect)).size.x, thing.def.uiIconOffset.y * ((Rect)(rect)).size.y);
			}
			else if (thing is Pawn || thing is Corpse)
			{
				Pawn pawn = thing as Pawn;
				if (pawn == null)
				{
					pawn = ((Corpse)thing).InnerPawn;
				}
				if (pawn.RaceProps.Humanlike)
				{
					rect = rect.ScaledBy(1.8f);
					rect.y += 3f;
					rect = rect.Rounded();
				}
			}

			if (alpha != 1f)
			{
				Color color2 = GUI.color;
				color2.a *= alpha;
				GUI.color = color2;
			}
			
			ThingIconWorker(rect, thing.def, resolvedIcon, resolvedIconAngle);
			GUI.color = Color.white;
		}

		private static void ThingIconWorker(Rect rect, ThingDef thingDef, Texture resolvedIcon, float resolvedIconAngle, float scale = 1f)
		{
			Vector2 texProportions = new Vector2(resolvedIcon.width, resolvedIcon.height);
			Rect texCoords = new Rect(0f, 0f, 1f, 1f); 
			if (thingDef.graphicData != null)
			{
				texProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
				if (thingDef.uiIconPath.NullOrEmpty() && thingDef.graphicData.linkFlags != 0)
				{
					texCoords = new Rect(0f, 0.5f, 0.25f, 0.25f);
				}
			}
			Widgets.DrawTextureFitted(rect, resolvedIcon, GenUI.IconDrawScale(thingDef) * scale, texProportions, texCoords, resolvedIconAngle);
		}

		public static readonly Texture2D RedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
		public static readonly Texture2D GreenTex = SolidColorMaterials.NewSolidColorTexture(Color.green);

		//Adaption of Verse.Widgets public static void DrawBox(Rect rect, int thickness = 1)
		//To enable passing the texture
		public static void DrawBox(Rect rect, Texture texture, int thickness = 1 )
		{
			Vector2 val = default(Vector2);
			val.x = rect.x;
			val.y = rect.y;
			Vector2 val2 = default(Vector2);
			val2.x = rect.x + rect.width;
			val2.y = rect.y + rect.height;
			if (val.x > val2.x)
			{
				float x = val.x;
				val.x = val2.x;
				val2.x = x;
			}
			if (val.y > val2.y)
			{
				float y = val.y;
				val.y = val2.y;
				val2.y = y;
			}
			Vector3 val3 = val2 - val;
			GUI.DrawTexture(new Rect(val.x, val.y, (float)thickness, val3.y), texture);
			GUI.DrawTexture(new Rect(val2.x - (float)thickness, val.y, (float)thickness, val3.y), texture);
			GUI.DrawTexture(new Rect(val.x + (float)thickness, val.y, val3.x - (float)(thickness * 2), (float)thickness), texture);
			GUI.DrawTexture(new Rect(val.x + (float)thickness, val2.y - (float)thickness, val3.x - (float)(thickness * 2), (float)thickness), texture);
		}


		public static void ListBox<T>(Rect ListBox_Outside, ref Vector2 scrollPos, ref int SelectedIndex, int selectedRef, List<T> inputList, Func<T, Rect, int, bool, int> func, Func<T, bool> SkippIf = null,float currY_Scroll_default = 60)
		{
			float itemHight = 30;
			float itemSeperation = 0;

			Widgets.DrawMenuSection(ListBox_Outside);

			var ListBox_Inside = ListBox_Outside;
			ListBox_Inside.width -= 20;

			//TODO
			ListBox_Inside.height = (itemHight + itemSeperation) * inputList.Count();


			Widgets.BeginScrollView(ListBox_Outside, ref scrollPos, ListBox_Inside);

			float currY_Scroll = ListBox_Outside.y + 5;
			var ValueRefItemRect = new Rect(ListBox_Inside.x + 5, currY_Scroll, ListBox_Inside.width, itemHight);
			int selectTemp = -1;

			for (int i = 0; i < inputList.Count(); i++)
			{
				if (SkippIf != null && SkippIf(inputList[i])) continue;
				ValueRefItemRect.y = currY_Scroll;
				selectTemp = func(inputList[i], ValueRefItemRect, i, selectedRef == i);
				if (selectTemp != -1)
				{
					SelectedIndex = selectTemp;
				}
				currY_Scroll += itemHight + itemSeperation;
			}

			Widgets.EndScrollView();
		}

		/// <summary>
		/// Creates a Scrollable List Box
		/// Design might need some improvements
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="posX">X Codinate of the ListBox</param>
		/// <param name="currentY">Current Y Position as ref</param>
		/// <param name="scrollPos">Scroll Positin inside</param>
		/// <param name="SelectedIndex">Currently Selected Index. -1 == Nothing Selected</param>
		/// <param name="selectedRef">Same as SelectedIndex but without ref (Why did i need that?)</param>
		/// <param name="inputList">List of Items to Display</param>
		/// <param name="func">Function that Draws each item in the List.  
		///	Parameters: (T ListItem , Rect rect, int i, bool selected)
		///	ListItem --> The Item to be displayed
		///	rect --> The Rect to display within
		/// i --> The Index of this Item
		/// selected --> True if this item has been selected
		/// </param>
		/// <param name="SkippIf">If not null then each entry where this returns True wont be displayed </param>
		public static void ListBox<T>(float posX, ref float currentY, ref Vector2 scrollPos, ref int SelectedIndex, int selectedRef, List<T> inputList, Func<T, Rect, int, bool, int> func, Func<T, bool> SkippIf = null)
		{
			float listBoxWidth = 250;
			var ListBox_Outside = new Rect(posX, currentY, listBoxWidth, 150);
			ListBox(ListBox_Outside, ref scrollPos, ref SelectedIndex, selectedRef, inputList, func, SkippIf);
		}



		}
}
