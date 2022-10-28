using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace cmod
{
    public class KoboldExtension : MonoBehaviour, ISavable
    {
        public string koboldName = "";

        public void Load(BinaryReader reader)
        {
            koboldName = reader.ReadString();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(koboldName);
        }

        public void Start()
        {
            string[] v = new string[] { "Ani", "Aya", "Asaka", "Avery", "Kerry", "Kozz" }; 
            koboldName = v[UnityEngine.Random.Range(0, v.Length)];
        }
    }
}

