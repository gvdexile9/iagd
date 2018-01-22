import * as React from 'react';
import { Provider, Store } from 'react-redux';
import './App.css';
import { GlobalState } from './types/index';
import MagicButton from './containers/MagicButton';
import { MockStore } from 'redux-mock-store';
import ItemContainer from './containers/ItemContainer';
import CornerMenu from './components/CornerMenu/CornerMenu';
import { isEmbedded } from './constants/index';

export interface Props {
  store: Store<GlobalState> | MockStore<GlobalState>;
}

class App extends React.Component<Props, object> {
  loading: boolean = false;

  constructor(props: Props) {
    super(props);
    this.props = props;
  }

  render() {
    const { store } = this.props;

    return (
      <Provider store={store}>
        <div className="App wrapper">
          <div id="content">
            <CornerMenu />
            {!isEmbedded ? <MagicButton label="Load mock items" /> : ''}
            <ItemContainer />
          </div>
        </div>
      </Provider>
    );
  }

}

export default App;
