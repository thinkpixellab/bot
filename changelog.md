# Bag of Tricks Change Log #

## Unreleased ##
* **NEW** `PropertyChangeWatcher` - *Issue #16*
* Added some usage details to `InstanceFactory`
* Added `Util.ThrowUnless`
* `Extensions.GetCustomAttributes` -> change param `MemberInfo` to more generic `ICustomAttributeProvider` - *Issue #19*
* Added `AddRange` to `ObservableCollectionPlus<T>` - *Issues # 17*
* Some work to get Windows Phone 7 projects to load, although I've hit a snag with sharing generic.xaml between SL4 and Phone - progress towards *Issue #18*

## v1.0.3 ##
* Much smarter implementation of `Util.GetHashCode`
* Added `ClearErrors` to `DataErrorHelper`
* Moved the `targets` files
* Removed `[DataContract]` from `Changeable` - and cried that serialization support in Silverlight wasn't more flexible
* `AsyncValue`: added `LoadCommand` property and `LoadError` event - *Closes #10*
