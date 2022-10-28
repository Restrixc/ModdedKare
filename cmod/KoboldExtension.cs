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
            string[] v = new string[] { "Ani", "Aya", "Asaka", "Avery", "Asan", "Alex", "Alisha", "Arin", "AJ", "Arey",
            "Berry", "Backstar", "Backster", "Bunyu", "Bonzi", "Bu", "Buck", "Bunz", "Bean",
            "Charlie", "Charles", "Chuck", "Chi", "Chan", "Chaza", "ChaCha", "Conner", "Carson",
            "Derick", "Danny", "Don", "Donny", "Dan", "David", "DJ", "Doc", "Dream",
            "Erin", "Eric", "Ean",
            "Foshi", "Foxen", "Ferren", "Farrar",
            "Gin", "Gold", "Grant", "Grand", "Gerran", "Gnar",
            "Herald", "Hex", "Hector", "Homer", "Hanz",
            "Ian", "Isle", "Island",
            "Jan", "Jay", "John", "Jermia", "Jerica", "Jerissa",
            "KJ", "Konner", "Kare", "Kobo",
            "Lan", "Lonner", "Loner",
            "Mike", "Mark", "Mince", "Meat", "Meet", "Mario", "Marry", "Mozio", "Moho",
            "Nike", "Nar",
            "Orissa", "Osira", "Owen", "Omen"}; 
            if(koboldName == "")
            {
                koboldName = v[UnityEngine.Random.Range(0, v.Length)] + " " + v[UnityEngine.Random.Range(0, v.Length)];
            }
        }

        public void Update()
        {
            gameObject.GetComponent<Kobold>().chatText.text = koboldName;
            gameObject.GetComponent<Kobold>().chatText.fontSize = 8;
            gameObject.GetComponent<Kobold>().chatText.fontSizeMax = 9;
            gameObject.GetComponent<Kobold>().chatText.fontSizeMin = 7;
            if ((Kobold)Photon.Pun.PhotonNetwork.LocalPlayer.TagObject == gameObject.GetComponent<Kobold>())
            {
                gameObject.GetComponent<Kobold>().chatText.alpha = 0;
            }
            else
            {
                Kobold k = (Kobold)Photon.Pun.PhotonNetwork.LocalPlayer.TagObject;
                if (k.transform.DistanceTo(gameObject.transform) < 10)
                {
                    gameObject.GetComponent<Kobold>().chatText.alpha = 1;
                }
                else
                {
                    gameObject.GetComponent<Kobold>().chatText.alpha = 0;
                }
            }
        }
    }
}

