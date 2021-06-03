using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WPM.ClipperLib {
				public partial class Clipper {

								const long MULTIPLIER = 5000000;

								public void AddPaths (List<Region> regions, PolyType polyType) {
												int regionCount = regions.Count;
												for (int k = 0; k < regionCount; k++) {
																AddPath (regions [k], polyType);
												}
								}

								public void AddPath (Region region, PolyType polyType) {
												if (region == null)
																return;
												int count = region.latlon.Length;
												List<IntPoint> points = new List<IntPoint> (count);
												for (int k = 0; k < count; k++) {
																IntPoint p = new IntPoint (region.latlon [k].x * MULTIPLIER, region.latlon [k].y * MULTIPLIER);
																points.Add (p);
												}
												AddPath (points, polyType, true);
								}

								public void Execute (ClipType clipType, IAdminEntity entity) {
												List<List<IntPoint>> solution = new List<List<IntPoint>> ();
												Execute (clipType, solution);
												int contourCount = solution.Count;
												entity.regions.Clear ();
												for (int c = 0; c < contourCount; c++) {
																List<IntPoint> points = solution [c];
																int count = points.Count;
																Vector2[] newPoints = new Vector2[count];
																for (int k = 0; k < count; k++) {
																				newPoints [k] = new Vector2 ((float)points [k].X / MULTIPLIER, (float)points [k].Y / MULTIPLIER);
																}
																Region region = new Region (entity, entity.regions.Count);
																region.UpdatePointsAndRect (newPoints);
																entity.regions.Add (region);
												}
								}

								public void Execute (ClipType clipType, Region output) {
												List<List<IntPoint>> solution = new List<List<IntPoint>> ();
												Execute (clipType, solution);
												int contourCount = solution.Count;
												if (contourCount==0) {
																output.Clear();
												} else {
																int best = 0;
																int pointCount = solution [0].Count;
																for (int k = 1; k < contourCount; k++) {
																				int candidatePointCount = solution [k].Count;
																				if (candidatePointCount > pointCount) {
																								pointCount = candidatePointCount;
																								best = k;
																				}
																}
																List<IntPoint> points = solution [best];
																int count = points.Count;
																Vector2[] newPoints = new Vector2[count];
																for (int k = 0; k < count; k++) {
																				newPoints [k] = new Vector2 ((float)points [k].X / MULTIPLIER, (float)points [k].Y / MULTIPLIER);
																}
																output.UpdatePointsAndRect (newPoints);
												}
								}
	

				}

}