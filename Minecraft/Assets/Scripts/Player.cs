using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public float ReachDistance = 3.0f;
    public LayerMask ReachMask;

    public float ExplosivePower = 3.0f;

    public Texture Crosshair = null;
    public float CrosshairScale = 1.0f;

    public GameObject ExplodeEffect = null;

	void Start ()
    {
	}
	
	void Update ()
    {
        if (Camera.main != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Ray CrosshairRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                AttackBlock(CrosshairRay);
            }
            else if (Input.GetButtonDown("Fire2"))
            {
                Ray CrosshairRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                PlaceBlock(CrosshairRay, VoxelType.Dirt);
            }
            else if (Input.GetButtonDown("Fire3"))
            {
                Ray CrosshairRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                ExplodeBlock(CrosshairRay, ExplosivePower);
            }
        }
	}

    void AttackBlock(Ray ray)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, ReachDistance, ReachMask))
        {
            if (hitInfo.collider is BoxCollider)
            {
                Voxel voxel = VoxelWorld.Inst.GetVoxelFromCollider(hitInfo.collider as BoxCollider);
                if (voxel != null)
                {
                    voxel.TakeDamage(1);
                }
            }
        }
    }

    void PlaceBlock(Ray ray, VoxelType type)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, ReachDistance, ReachMask))
        {
            if (hitInfo.collider is BoxCollider)
            {
                Voxel voxel = VoxelWorld.Inst.GetVoxelFromCollider(hitInfo.collider as BoxCollider);
                if (voxel != null)
                {
                    float threshold = 0.1f;
                    IntVec3 offset = new IntVec3(0, 0, 0);
                    if (hitInfo.normal.y > threshold)
                        offset.Y = 1;
                    else if (hitInfo.normal.x > threshold)
                        offset.X = 1;
                    else if (hitInfo.normal.z > threshold)
                        offset.Z = 1;
                    else if (hitInfo.normal.y < -threshold)
                        offset.Y = -1;
                    else if (hitInfo.normal.x < -threshold)
                        offset.X = -1;
                    else if (hitInfo.normal.z < -threshold)
                        offset.Z = -1;
                    IntVec3 placePos = voxel.Position.Offset(offset);
                    if (VoxelWorld.Inst.IsVoxelWorldIndexValid(placePos.X, placePos.Y, placePos.Z))
                    {
                        Voxel placeVoxel = VoxelWorld.Inst.GetVoxel(placePos);
                        if (placeVoxel.TypeDef.Type == VoxelType.Air)
                        {
                            placeVoxel.SetType(type);
                            VoxelWorld.Inst.Refresh();
                        }
                    }
                }
            }
        }
    }

    void ExplodeBlock(Ray ray, float radius)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, ReachDistance, ReachMask))
        {
            if (hitInfo.collider is BoxCollider)
            {
                List<Voxel> voxels = VoxelWorld.Inst.GetVoxels(hitInfo.point, radius);
                voxels.ForEach(vox => vox.SetType(VoxelType.Air));
                VoxelWorld.Inst.Refresh();

                GameObject.Instantiate(ExplodeEffect, hitInfo.point, Quaternion.identity);
            }
        }
    }

    void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float halfWidth = Crosshair.width * 0.5f * CrosshairScale;
        float halfHeight = Crosshair.height * 0.5f * CrosshairScale;
        GUI.DrawTexture(new Rect(cx - halfWidth, cy - halfHeight, Crosshair.width * CrosshairScale, Crosshair.height * CrosshairScale), Crosshair);
    }
}
