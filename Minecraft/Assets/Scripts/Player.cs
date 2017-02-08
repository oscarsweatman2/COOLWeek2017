using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public int Energy = 3;
    public float EnergyGainRate = 2.0f;
    public float EnergyTimer = 0;
    public float ReachDistance = 3.0f;
    public LayerMask ReachMask;

    public int StrongCost = 5;
    public int WeakCost = 3;

    public Texture Crosshair = null;
    public float CrosshairScale = 1.0f;

    private VoxelType CurrentBlock = VoxelType.Weak;

	void Start ()
    {
        EnergyTimer = EnergyGainRate;
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
                PlaceBlock(CrosshairRay, CurrentBlock);
            }
            else if (Input.GetButtonDown("Fire3"))
            {
                if (CurrentBlock == VoxelType.Weak)
                {
                    CurrentBlock = VoxelType.Strong;
                }
                else
                {
                    CurrentBlock = VoxelType.Weak;
                }
            }
        }
        GainEnergy();
	}

    void AttackBlock(Ray ray)
    { 
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, ReachDistance, ReachMask))
        {
            if (hitInfo.collider is BoxCollider)
            {
                Voxel voxel = VoxelWorld.Inst.GetVoxelFromCollider(hitInfo.collider as BoxCollider);
                if (voxel.TypeDef.Type != VoxelType.Weak && voxel.TypeDef.Type != VoxelType.Strong)
                {
                    return;
                }
                if (voxel != null)
                {
                    if (Energy > 0)
                    {
                        voxel.TakeDamage(1);
                        Energy -= 1;
                    }
                }
            }
        }
    }

    void PlaceBlock(Ray ray, VoxelType type)
    {
        
        if (type != VoxelType.Strong && type != VoxelType.Weak)
        {
            return;
        }
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
                            if (type == VoxelType.Strong)
                            {
                                if (Energy >= StrongCost)
                                {
                                    placeVoxel.SetType(type);
                                    VoxelWorld.Inst.Refresh();
                                    Energy -= StrongCost;
                                }
                            }
                            else
                            {
                                if (Energy >= WeakCost)
                                {
                                    placeVoxel.SetType(type);
                                    VoxelWorld.Inst.Refresh();
                                    Energy -= WeakCost;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void GainEnergy()
    {
        EnergyTimer -= Time.deltaTime;
        if (EnergyTimer <= 0)
        {
            Energy += 1;
            EnergyTimer = EnergyGainRate;
        }
    }

    void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float halfWidth = Crosshair.width * 0.5f * CrosshairScale;
        float halfHeight = Crosshair.height * 0.5f * CrosshairScale;
        GUI.DrawTexture(new Rect(cx - halfWidth, cy - halfHeight, Crosshair.width * CrosshairScale, Crosshair.height * CrosshairScale), Crosshair);
        GUI.Label(new Rect(0, 0, 100, 100), Energy.ToString());
    }
}
