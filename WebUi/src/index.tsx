import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { IntlProvider } from 'react-intl';
import App from './App';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import IItem from './interfaces/IItem';
import { Store } from 'react-redux';

import { createStore } from 'redux';
import { GlobalState } from './types/index';
import { setItemReducer } from './reducers/setItemReducer';
import { setItems, addItems, setLoadingStatus } from './actions';

/// https://redux.js.org/docs/api/combineReducers.html
const enhancer = window['devToolsExtension'] ? window['devToolsExtension']()(createStore) : createStore;

const store: Store<GlobalState> = enhancer(setItemReducer, {
  clickCounter: 1,
  items: [],
  isLoading: false
});

/* == BEGIN MAGIC ==
* This little piece of magic enables the following function to be called globally:;
  data.globalStore.dispatch(data.globalSetItems());
  This allows CEFSharp embedded to update the redux state of the app
*/
declare abstract class data {
  public static globalStore: {};
  public static globalSetItems(items: IItem[]): {};
  public static globalAddItems(items: IItem[]): {};
  public static globalSetIsLoading(v: boolean): {};
}
if (typeof data === 'object') {
  data.globalStore = store;
  data.globalSetItems = setItems;
  data.globalAddItems = addItems;
  data.globalSetIsLoading = setLoadingStatus;
}
/* == END MAGIC == */

const locale = 'en';
ReactDOM.render(
  <IntlProvider locale={locale}>
    <App store={store} />
  </IntlProvider>,
  document.getElementById('root') as HTMLElement
);
registerServiceWorker();
