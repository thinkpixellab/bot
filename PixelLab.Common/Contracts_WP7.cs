
namespace System.Diagnostics.Contracts {
  public class Contract {
    public static void Requires(bool truth) {
      if (!truth) {
        throw new Exception("Requires failed.");
      }
    }
    public static void Requires<T>(bool truth) {
      Requires(truth);
    }
    public static T Result<T>() {
      return default(T);
    }
    public static void Invariant(bool truth){}
    public static void Ensures(bool truth) { }
    public static void Assume(bool truth) { }
  }

  public class PureAttribute : Attribute {

  }
  public class ContractInvariantMethodAttribute : Attribute {

  }
  public class ContractClassForAttribute : Attribute {
    public ContractClassForAttribute(Type t) { }

  }
  public class ContractClassAttribute : Attribute {
    public ContractClassAttribute(Type t) { }
  }

}
