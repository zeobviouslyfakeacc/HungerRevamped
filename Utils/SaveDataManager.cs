using ModData;

namespace HungerRevamped
{
    internal class SaveDataManager
    {

        ModDataManager dm = new ModDataManager("HungerRevamped");
        
        public bool Save(string data)
        {
            return dm.Save(data, null);
        }

        public string? LoadData()
        {
            string? data = dm.Load(null);
            return data;
        }

    }
}
