using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Name", menuName = "CharacterDataSO")]
public class CharacterData : ScriptableObject
{
    [Header("Properties")]
    public float jumpHeight; // How high the character can jump

    public int AirDashesUsed;
    public int AirDashesAvailable;
    public int JumpsUsed;
    public int JumpsAvailable;

    [Header("Normals")]
    public List<NormalAttack> LightNormals = new List<NormalAttack>(); // The characters light normal attacks
    public List<NormalAttack> MediumNormals = new List<NormalAttack>(); // The characters medium normal attacks
    public List<NormalAttack> HeavyNormals = new List<NormalAttack>(); // The characters heavy normal attacks
    public List<NormalAttack> AirNormals = new List<NormalAttack>(); // The characters normal attacks while airborne

    [Header("Uniques")]
    public List<UniqueMove> Uniques = new List<UniqueMove>(); // The characters unique moves

    [Header("Specials")]
    public List<SpecialMove> Specials = new List<SpecialMove>(); // The characters special moves
}