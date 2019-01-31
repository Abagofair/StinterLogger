using StinterLogger.FuelCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger
{
    public class ModelManager
    {
        private IDictionary<ModelType, IEntityModel> _models;

        public ModelManager()
        {
            this._models = new Dictionary<ModelType, IEntityModel>();
        }

        public IEntityModel GetModel(ModelType modelType)
        {
            IEntityModel model = null;
            bool found = this._models.TryGetValue(modelType, out model);
            return model;
        }

        public void DestroyModel(ModelType modelType)
        {
            if (!this._models.ContainsKey(modelType))
            {
                throw new Exception("Model doesn't exist");
            }
            else
            {
                this._models.Remove(modelType);
            }
        }

        public IEntityModel CreateModel(ModelType modelType)
        {
            IEntityModel model = null;
            if (modelType == ModelType.FuelModel)
            {
                if (this._models.ContainsKey(ModelType.FuelModel))
                {
                    throw new Exception("Model already exists");
                }
                else
                {
                    model = new FuelModel();
                    this._models.Add(ModelType.FuelModel, model);
                }
            }
            return model;
        }
    }
}
