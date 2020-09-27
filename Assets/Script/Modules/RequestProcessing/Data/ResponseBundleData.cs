using Strx.Expansions.Modules.RequestProcessing.Data;
using UnityEngine;

namespace Modules.RequestProcessing.Data
{
    public class ResponseBundleData : ResponseData
    {
        public AssetBundle AssetBundle { get; private set; }

        public byte[] Data { get; private set; }

        public ResponseBundleData SetAssetBundle(AssetBundle bundle)
        {
            AssetBundle = bundle;
            return this;
        }

        public ResponseBundleData SetRawData(byte[] data)
        {
            Data = data;
            return this;
        }
    }
}