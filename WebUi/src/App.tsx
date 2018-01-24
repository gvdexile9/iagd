import * as React from 'react';
import { Provider, Store } from 'react-redux';
import {Tab, TabList, TabPanel, Tabs} from 'react-tabs';
import { GlobalState } from './types/index';
import MagicButton from './containers/MagicButton';
import { MockStore } from 'redux-mock-store';
import ItemContainer from './containers/ItemContainer';
import CornerMenu from './components/CornerMenu/CornerMenu';
import { isEmbedded } from './constants/index';
import 'react-tabs/style/react-tabs.css'; // scss and less also available
import './App.css';
import { requestInitialItems } from './actions';

export interface Props {
  store: Store<GlobalState> | MockStore<GlobalState>;
}

class App extends React.Component<Props, object> {
  loading: boolean = false;

  constructor(props: Props) {
    super(props);
    this.props = props;
    this.props.store.dispatch(requestInitialItems());
  }

  render() {
    const { store } = this.props;

    return (
      <Provider store={store}>
        <div className="App wrapper">
          <div id="content">
            <Tabs className="tab-control">
              <div className="tab-list">
                <TabList>
                  <Tab>Items</Tab>
                  <Tab>Crafting</Tab>
                </TabList>
                <CornerMenu />
              </div>

              <TabPanel>
                {!isEmbedded ? <MagicButton label="Load mock items" /> : ''}
                <ItemContainer />
              </TabPanel>

              <TabPanel>
                TODO: Crafting is not implemented in this build!
              </TabPanel>
            </Tabs>
          </div>
        </div>
      </Provider>
    );
  }

}

export default App;
