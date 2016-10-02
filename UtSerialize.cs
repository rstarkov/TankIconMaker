using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RT.Util.Serialization;
using D = System.Drawing;
using W = System.Windows.Media;

namespace TankIconMaker
{
    /// <summary>
    /// Enables <see cref="XmlClassify"/> to save color properties as strings of a human-editable form.
    /// </summary>
    sealed class colorTypeOptions : ClassifyTypeOptions,
        IClassifySubstitute<W.Color, string>,
        IClassifySubstitute<D.Color, string>
    {
        public W.Color FromSubstitute(string instance)
        {
            return (this as IClassifySubstitute<D.Color, string>).FromSubstitute(instance).ToColorWpf();
        }

        public string ToSubstitute(W.Color instance)
        {
            return ToSubstitute(instance.ToColorGdi());
        }

        D.Color IClassifySubstitute<D.Color, string>.FromSubstitute(string instance) { return FromSubstituteD(instance); }
        private D.Color FromSubstituteD(string instance)
        {
            if (instance == null || !instance.StartsWith("#") || (instance.Length != 7 && instance.Length != 9))
                throw new ClassifyDesubstitutionFailedException();

            try
            {
                int alpha = instance.Length == 7 ? 255 : int.Parse(instance.Substring(1, 2), NumberStyles.HexNumber);
                int r = int.Parse(instance.Substring(instance.Length == 7 ? 1 : 3, 2), NumberStyles.HexNumber);
                int g = int.Parse(instance.Substring(instance.Length == 7 ? 3 : 5, 2), NumberStyles.HexNumber);
                int b = int.Parse(instance.Substring(instance.Length == 7 ? 5 : 7, 2), NumberStyles.HexNumber);
                return D.Color.FromArgb(alpha, r, g, b);
            }
            catch
            {
                throw new ClassifyDesubstitutionFailedException();
            }
        }

        public string ToSubstitute(D.Color instance)
        {
            return instance.A == 255 ? "#{0:X2}{1:X2}{2:X2}".Fmt(instance.R, instance.G, instance.B) : "#{0:X2}{1:X2}{2:X2}{3:X2}".Fmt(instance.A, instance.R, instance.G, instance.B);
        }
    }


    /// <summary>
    /// Filters lists of <see cref="LayerBase"/> objects before XmlClassify attempts to decode them, removing all
    /// entries pertaining to layer types that no longer exist in the assembly and hence can't possibly be instantiated.
    /// </summary>
    sealed class listLayerBaseOptions : ClassifyTypeOptions, IClassifyXmlTypeProcessor
    {
        void IClassifyTypeProcessor<XElement>.AfterSerialize(object obj, XElement element) { }
        void IClassifyTypeProcessor<XElement>.AfterDeserialize(object obj, XElement element) { }
        void IClassifyTypeProcessor<XElement>.BeforeSerialize(object obj) { }
        void IClassifyTypeProcessor<XElement>.BeforeDeserialize(XElement element)
        {
            foreach (var item in element.Nodes().OfType<XElement>().Where(e => e.Name == "item").ToArray())
            {
                var type = item.Attribute("type");
                if (type == null)
                    item.Remove();
                else if (!App.LayerTypes.Any(lt => lt.Type.Name == type.Value || lt.Type.FullName == type.Value))
                    item.Remove();
            }
        }
    }

    /// <summary>
    /// Filters lists of <see cref="EffectBase"/> objects before XmlClassify attempts to decode them, removing all
    /// entries pertaining to layer types that no longer exist in the assembly and hence can't possibly be instantiated.
    /// </summary>
    sealed class listEffectBaseOptions : ClassifyTypeOptions, IClassifyXmlTypeProcessor
    {
        void IClassifyTypeProcessor<XElement>.AfterSerialize(object obj, XElement element) { }
        void IClassifyTypeProcessor<XElement>.AfterDeserialize(object obj, XElement element) { }
        void IClassifyTypeProcessor<XElement>.BeforeSerialize(object obj) { }
        void IClassifyTypeProcessor<XElement>.BeforeDeserialize(XElement element)
        {
            foreach (var item in element.Nodes().OfType<XElement>().Where(e => e.Name == "item").ToArray())
            {
                var type = item.Attribute("type");
                if (type == null)
                    item.Remove();
                else if (type.Value == "TankIconMaker.Effects.BrightnessAdjustmentEffect")
                    type.Value = "TankIconMaker.Effects.NormalizeBrightnessEffect";
                else if (type.Value == "TankIconMaker.Effects.ModulateEffect")
                    type.Value = "TankIconMaker.Effects.HueSaturationLightnessEffect";
                else if (!App.EffectTypes.Any(lt => lt.Type.Name == type.Value || lt.Type.FullName == type.Value))
                    item.Remove();
            }
        }
    }
}
