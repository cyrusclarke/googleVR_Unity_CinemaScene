// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// Draws a circular reticle in front of any object that the user gazes at.
/// The circle dilates if the object is clickable.
[AddComponentMenu("GoogleVR/UI/GvrReticle")]
[RequireComponent(typeof(Renderer))]
public class GvrReticle : MonoBehaviour, IGvrGazePointer {
  /// Number of segments making the reticle circle.
  public int reticleSegments = 20;

  /// Growth speed multiplier for the reticle/
  public float reticleGrowthSpeed = 8.0f;

  // Private members
  private Material materialComp;
  private GameObject targetObj;
  private string target;
  public int counter1 = 0;
  public int counter2 = 0;
  public int counter3 = 0;
  public int counter4 = 0; 




  // Current inner angle of the reticle (in degrees).
  private float reticleInnerAngle = 0.0f;
  // Current outer angle of the reticle (in degrees).
  private float reticleOuterAngle = 1.5f;
  // Current distance of the reticle (in meters).
  private float reticleDistanceInMeters = 10.0f;

  // Minimum inner angle of the reticle (in degrees).
  private const float kReticleMinInnerAngle = 0.0f;
  // Minimum outer angle of the reticle (in degrees).
  private const float kReticleMinOuterAngle = 1.0f;
  // Angle at which to expand the reticle when intersecting with an object
  // (in degrees).
  private const float kReticleGrowthAngle = 1.5f;

  // Minimum distance of the reticle (in meters).
  private const float kReticleDistanceMin = 0.45f;
  // Maximum distance of the reticle (in meters).
  private const float kReticleDistanceMax = 10.0f;

  // Current inner and outer diameters of the reticle,
  // before distance multiplication.
  private float reticleInnerDiameter = 0.0f;
  private float reticleOuterDiameter = 0.0f;

/// create a float called gazeStartTime
/// and an object called gazedAt so we can keep track of the object and how long we are gazing at it.
/// </summary>
private float gazeStartTime;
private GameObject gazedAt;

  void Start () {
    CreateReticleVertices();

    materialComp = gameObject.GetComponent<Renderer>().material;
		//Set gazeStartTime to -1 and no object to be gazed at when we start
		gazeStartTime = -1f; 
		gazedAt = null; 
  }

  void OnEnable() {
    GazeInputModule.gazePointer = this;
  }

  void OnDisable() {
    if (GazeInputModule.gazePointer == this) {
      GazeInputModule.gazePointer = null;
    }
  }

  void Update() {

    UpdateDiameters();
  }

  /// This is called when the 'BaseInputModule' system should be enabled.
  public void OnGazeEnabled() {

  }

  /// This is called when the 'BaseInputModule' system should be disabled.
  public void OnGazeDisabled() {

  }

  /// Called when the user is looking on a valid GameObject. This can be a 3D
  /// or UI element.
  ///
  /// The camera is the event camera, the target is the object
  /// the user is looking at, and the intersectionPosition is the intersection
  /// point of the ray sent from the camera on the object.
  public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                          bool isInteractive) {
    SetGazeTarget(intersectionPosition, isInteractive);
		//gazedAt is set to our target object and gazeStartTime will be the time recorderd since the start of game
		gazedAt = targetObject;
		gazeStartTime = Time.time;
//		ExecuteEvents.Execute(gazedAt, null, (TimedInputHandler handler, BaseEventData data) => handler.TimerStart ());

  }

  /// Called every frame the user is still looking at a valid GameObject. This
  /// can be a 3D or UI element.
  ///
  /// The camera is the event camera, the target is the object the user is
  /// looking at, and the intersectionPosition is the intersection point of the
  /// ray sent from the camera on the object.
  public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                         bool isInteractive) {
    SetGazeTarget(intersectionPosition, isInteractive);
		//Resets the gazeStart to -1 each time the event is triggered --> if gazedAt is not nothing and the start time is not zero we want to reset (DISABLED)
		if (gazedAt != null && gazeStartTime > 0f) {
//			Debug.Log (targetObject);
			target = targetObject.ToString ();
//			Debug.Log (target + (Time.time - gazeStartTime));
//			Debug.Log(Time.time - gazeStartTime);
			///check if gaze is at least 3 seconds AND in second part we stop the event from triggering more than once on the same object  
			/// if gaze is >3s we execute the events using the handler. here we execute the handletimeinput function. 
			if (Time.time - gazeStartTime > 3.0f && ExecuteEvents.CanHandleEvent<TimedInputHandler> (gazedAt)) {
				//reset start time if wanted
//				gazeStartTime = -1f;
				///execute TimedInputHandler and check if the gazed at is equal to null
				ExecuteEvents.Execute (gazedAt, null, (TimedInputHandler handler, BaseEventData data) => handler.HandleTimeInput ());
//				Debug.Log (counter);
				if (target == "Video1 (UnityEngine.GameObject)") {
					counter1++;
					Debug.Log (counter1 + "Video1");
				} else if (target == "Video2 (UnityEngine.GameObject)") {
					counter2++;
					Debug.Log (counter2 + "Video2");
				} else if (target == "Video3 (UnityEngine.GameObject)") {
					counter3++;
					Debug.Log (counter3 + "Video3");
				} else if (target == "Video4 (UnityEngine.GameObject)") {
					counter4++;
					Debug.Log (counter4 + "Video4");
				} else {
					Debug.Log ("NO VALUE READ");
				}
		
			}
		}
  }

  /// Called when the user's look no longer intersects an object previously
  /// intersected with a ray projected from the camera.
  /// This is also called just before **OnGazeDisabled** and may have have any of
  /// the values set as **null**.
  ///
  /// The camera is the event camera and the target is the object the user
  /// previously looked at.
  public void OnGazeExit(Camera camera, GameObject targetObject) {
    reticleDistanceInMeters = kReticleDistanceMax;
    reticleInnerAngle = kReticleMinInnerAngle;
    reticleOuterAngle = kReticleMinOuterAngle;
		ExecuteEvents.Execute(gazedAt, null, (TimedInputHandler handler, BaseEventData data) => handler.HandleTimeOutput ());

  }

  /// Called when a trigger event is initiated. This is practically when
  /// the user begins pressing the trigger.
  public void OnGazeTriggerStart(Camera camera) {
    // Put your reticle trigger start logic here :)
  }

  /// Called when a trigger event is finished. This is practically when
  /// the user releases the trigger.
  public void OnGazeTriggerEnd(Camera camera) {
    // Put your reticle trigger end logic here :)
  }

  public void GetPointerRadius(out float innerRadius, out float outerRadius) {
    float min_inner_angle_radians = Mathf.Deg2Rad * kReticleMinInnerAngle;
    float max_inner_angle_radians = Mathf.Deg2Rad * (kReticleMinInnerAngle + kReticleGrowthAngle);

    innerRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
    outerRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
  }

  private void CreateReticleVertices() {
    Mesh mesh = new Mesh();
    gameObject.AddComponent<MeshFilter>();
    GetComponent<MeshFilter>().mesh = mesh;

    int segments_count = reticleSegments;
    int vertex_count = (segments_count+1)*2;

    #region Vertices

    Vector3[] vertices = new Vector3[vertex_count];

    const float kTwoPi = Mathf.PI * 2.0f;
    int vi = 0;
    for (int si = 0; si <= segments_count; ++si) {
      // Add two vertices for every circle segment: one at the beginning of the
      // prism, and one at the end of the prism.
      float angle = (float)si / (float)(segments_count) * kTwoPi;

      float x = Mathf.Sin(angle);
      float y = Mathf.Cos(angle);

      vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
      vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
    }
    #endregion

    #region Triangles
    int indices_count = (segments_count+1)*3*2;
    int[] indices = new int[indices_count];

    int vert = 0;
    int idx = 0;
    for (int si = 0; si < segments_count; ++si) {
      indices[idx++] = vert+1;
      indices[idx++] = vert;
      indices[idx++] = vert+2;

      indices[idx++] = vert+1;
      indices[idx++] = vert+2;
      indices[idx++] = vert+3;

      vert += 2;
    }
    #endregion

    mesh.vertices = vertices;
    mesh.triangles = indices;
    mesh.RecalculateBounds();
    mesh.Optimize();
  }

  private void UpdateDiameters() {
    reticleDistanceInMeters =
      Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

    if (reticleInnerAngle < kReticleMinInnerAngle) {
      reticleInnerAngle = kReticleMinInnerAngle;
    }

    if (reticleOuterAngle < kReticleMinOuterAngle) {
      reticleOuterAngle = kReticleMinOuterAngle;
    }

    float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
    float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

    float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
    float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

    reticleInnerDiameter =
        Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
    reticleOuterDiameter =
        Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

    materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
    materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
    materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
  }

  private void SetGazeTarget(Vector3 target, bool interactive) {
    Vector3 targetLocalPosition = transform.InverseTransformPoint(target);

    reticleDistanceInMeters =
        Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);
    if (interactive) {
      reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthAngle;
      reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthAngle;
    } else {
      reticleInnerAngle = kReticleMinInnerAngle;
      reticleOuterAngle = kReticleMinOuterAngle;
    }
  }
}
