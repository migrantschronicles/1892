
namespace WPM {

    public delegate bool AttribPredicate(JSONObject json);

    public interface IExtendableAttribute {
        bool hasAttributes { get; }
        JSONObject attrib { get; set; }
    }
}
