using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;


public class AutoSimObject : EditorWindow {
  [MenuItem("AI2-THOR/Make Sim Object")]
  static void MakeSimObject() {
    string basePath = "Assets/Prefabs/GoogleScannedObjects/";
    string modelId = "TOOL_BELT";
    // int NUM_COLLIDERS = 4;

    // load the annotations
    var annotationsStr = System.IO.File.ReadAllText(basePath + modelId + "/annotations.json");
    JObject annotations = JObject.Parse(annotationsStr);

    // instantiate the prefab
    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + modelId + "/model.obj");
    GameObject obj = GameObject.Instantiate(prefab);
    obj.name = modelId;

    // add a SimObjPhysics component
    SimObjPhysics simObj = obj.AddComponent<SimObjPhysics>();
    simObj.assetID = modelId;
    simObj.PrimaryProperty = (SimObjPrimaryProperty)Enum.Parse(
      typeof(SimObjPrimaryProperty), annotations["primaryProperty"].ToString()
    );

    // add the visibility points
    GameObject visPoints = new GameObject("visibilityPoints");
    visPoints.transform.parent = obj.transform;
    Transform[] visPointsTransforms = new Transform[annotations["visibilityPoints"].Count()];
    for (int i = 0; i < annotations["visibilityPoints"].Count(); i++) {
      GameObject visPoint = new GameObject("visibilityPoint" + i);
      visPoint.transform.parent = visPoints.transform;
      visPoint.transform.localPosition = new Vector3(
        (float)annotations["visibilityPoints"][i]["x"],
        (float)annotations["visibilityPoints"][i]["y"],
        (float)annotations["visibilityPoints"][i]["z"]
      );
      visPointsTransforms[i] = visPoint.transform;
    }
    simObj.VisibilityPoints = visPointsTransforms;

    GameObject meshColliders = new GameObject("colliders");
    meshColliders.transform.parent = obj.transform;
    MeshCollider meshCollider = meshColliders.AddComponent<MeshCollider>();
    meshCollider.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
      basePath + modelId + "/collider.obj"
    );
    meshCollider.convex = true;

    Collider[] colliders = new Collider[1];
    colliders[0] = meshCollider;

    // add the 4 mesh colliders
    // Collider[] colliders = new Collider[NUM_COLLIDERS];
    // for (int i = 0; i < NUM_COLLIDERS; i++) {
    //   MeshCollider meshCollider = meshColliders.AddComponent<MeshCollider>();
    //   meshCollider.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
    //     basePath + modelId + "/decomp_" + i + ".obj"
    //   );
    //   meshCollider.convex = true;
    //   colliders[i] = meshCollider;
    // }
    simObj.MyColliders = colliders;

    // add a RigidBody component
    Rigidbody rigidBody = obj.AddComponent<Rigidbody>();

    // set the transform rotation
    obj.transform.rotation = Quaternion.Euler(
      (float)annotations["transform"]["rotation"]["x"],
      (float)annotations["transform"]["rotation"]["y"],
      (float)annotations["transform"]["rotation"]["z"]
    );

    // set the transform scale
    obj.transform.localScale = new Vector3(
      (float)annotations["transform"]["scale"]["x"],
      (float)annotations["transform"]["scale"]["y"],
      (float)annotations["transform"]["scale"]["z"]
    );

    // save obj as a prefab
    PrefabUtility.SaveAsPrefabAsset(obj, basePath + modelId + "/" + modelId + ".prefab");
  }
}
