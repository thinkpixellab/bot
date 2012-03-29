# Bag of Tricks Change Log #

## V5.2.0 - 2012-03-23
* Added `Clone` extention method for `IDictionary`
* Added some helpers for enums
* Added 'unpacked' support to `ListReorderUtil`
* `ReorderListBox` learned orientation. *You're welcome, John.*
* Better exceptions when one uses `ModalControl.Close` incorrectly
* Can now opt-in to allowing parent content to close nested popups in `ModalControl`
* Added conversion from `CompareInfo` to `IComparer<string>`
* Added `ConfigFactory`. I have customer reasons for this. Could use a lot of work, but it's a good start.

## V5.1.0 - 2011-11-07
*A bunch of new stuff, but no breaking changes. So just a dot release.*

* Added a few `Enum` helpers to `Util`
* Used `Changeable.UpdateProperty` in more places
* NEW! `SpinningProgressControl`
* NEW! `ScrollBehavior`
* Fixes for `ModalControl`
* Fixed some weirdness in Core SL4 proj file

## V5.0.0 - 2011-09-19
*No huge changes or additions, but there is a big breaking change with AsyncValue<T>. Soooo...new major version number.*

* Fixed silly naming in Util around `InterlockedSetIfNotNull` vs `InterlockedSetNullField`.
* Huge code review for `PanZoomControl`
* Bug fixes and clean-up in `SortHelper`
* **BREAKING** - `IAsyncValue<T>` and friends now raise `ApplicationUnhandledExceptionEventArgs`. If cancel is not set, an exception is thrown.

## V4.1.0 - 2011-07-20
*Lot's of fun new stuff, but no breaking changes! Feels good.*

* Added `GetStringComparer` extension that takes a CultureInfo
* Added `SelectAdjacentPairs` to extensions
* Added `ResourceHelpers` extensions to SL assembly
* Added `TryGetTypedValue<>` extension
* Fixed a gnarly bug in SL version of `ColorHelper.HsbToRgb`
* Added `NextFloat` extension method for `Random`
* Added `ModalControl` *Closes #23*
* Adjacent pairs methods now take `IEnumerable<T>` instead of requiring `IList<T>`

## v4.0.0 - 2011-07-08

* **BREAKING** - Removed `DispatcherExtensions`. The implementation was trivial and it brought in all of Reactive Extensions as a dependency, which isn't worth it.
* Fix for *Issue #3* from Larry.

## v3.0.0 - 2011-07-08

*Because this rev contains non-compatible breaking changes, I'm bumping the version number. If this is annoying, let me know and I'll be more careful.*

* Added command support to `DoubleClickBehavior` - *thanks, Larry*
* Added demo for `DoubleClickBehavior`
* Added `FilteredObservableEnumerable`
* **BREAKING** - Using *Reactive Extensions* for async features

## v2.0.0 - 2011-07-05

*In the `NuGet` world, BOT is now three projects: Core, Common, BOT. This allows one to minimize dependencies on external assemblies (mostly Prism) when not needed.*

* **BREAKING** - Added a `Core` Silverlight assembly to match the same in WPF
* **BREAKING** - Moved `DemoCollection` back to WPF demo app. This eliminates the dependency on Prism in Core and Common

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
