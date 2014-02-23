using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PoleSagTool
{
    public static class TransactionExt
    {
        public static IList<ObjectId> GetAllEntities(this Transaction tr)
        {
            var result = new List<ObjectId>();
            var bt = tr.GetObject(
                AcadApp.DB.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr = tr.GetObject(
                bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

            foreach (ObjectId id in btr)
            {
                result.Add(id);
            }
            return result;
        }

        public static BlockReference InsertBlock(this Transaction trans, ObjectId blockDefId)
        {
            return trans.InsertBlock(blockDefId, Point3d.Origin);
        }

        public static BlockReference InsertBlock(this Transaction trans, ObjectId blockDefId, Point3d position)
        {
            var result = new BlockReference(position, blockDefId);
            ObjectId blockId = trans.InsertEntity(result);

            var btr = trans.GetObject(
                blockDefId, OpenMode.ForRead) as BlockTableRecord;

            // Add the attributes
            foreach (ObjectId attId in btr)
            {
                var ad = trans.GetObject(
                    attId, OpenMode.ForRead) as AttributeDefinition;
                if (ad != null)
                {
                    AttributeReference ar = new AttributeReference();
                    ar.SetAttributeFromBlock(ad, result.BlockTransform);
                    result.AttributeCollection.AppendAttribute(ar);
                    trans.AddNewlyCreatedDBObject(ar, true);
                }
            }

            return result;
        }

        public static ObjectId InsertEntity(this Transaction trans, Entity entity)
        {
            ObjectId entId = ObjectId.Null;
            using (DocumentLock loc = AcadApp.CurDoc.LockDocument())
            {
                BlockTableRecord curSpace = trans.GetObject(
                    AcadApp.DB.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                if (curSpace != null)
                {
                    entId = curSpace.AppendEntity(entity);
                    trans.AddNewlyCreatedDBObject(entity, true);
                }
            }
            return entId;
        }
    }
}
