# Bag of Tricks Change Log #

## v1.1.0 - 2011-06-22 ##

*Moving to [Semantic Versioning](http://semver.org/). Non-breaking features are added, so we bump minor version number.*

### Library changes ###

* Added `Insert` method to `WeakEnumerable<T>`, which puts new items at the beginning of the enumeration. Which makes insertion O(1) instead of O(n)
* Added `SortedObservableEnumerable<T>`: because sometimes you'd like a sorted view of an `INotifyCollectionChange`
* Added `Synchronize` extension method. For those times you want to update an OCP<T> with a source collection and a factory

### Non-library changes ###

* Fixed the WPF demo around property watcher
* Removed Phone from the main solution (since it remains broken)
* Added another layer of directories to builds, so there are no longer collisions between Silverlight and WPF build outputs

## v1.0.6 ##
* Added support for critical inner exceptions to `IsCritical`
* Moved `DispatcherExtensions` methods to use `SynchronizationContext`, which is more general
  * Should likely rename the class to `SynchronizationContextExtensions`, huh?
* Naming and param order changes to `PropertyChangeWatcher` - **Breaking Change**
* Clean-up in the SL test project
* Added test for `PropertyChangeWatcher`

## v1.0.5 ##
* A mountain of changes to `ObservableCollectionPlus<T>`
  * Added `Reset` method - *Closes #21*
  * Made sort methods safe - *Closes #22*
  * Added `MultiUpdateActive`
  * Changed the location of the call to protected `AfterMultiUpdate` to before raising reset

## v1.0.4 ##
* A mountain of clean-up in demo and test code
* Added `WatchProperty` extension method to `Extensions`
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
