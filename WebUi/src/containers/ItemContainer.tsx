import * as React from 'react';
import Item from '../components/Item';
import IItem from '../interfaces/IItem';
import { connect } from 'react-redux';
import './ItemContainer.css';
import * as ReactTooltip from 'react-tooltip';
import { isEmbedded } from '../constants';
import Spinner from '../components/Spinner';
import OnScrollLoader from './InfiniteItemLoader';
import { GlobalReducerState } from '../types';
import translate from '../translations/EmbeddedTranslator';
import { copyToClipboard } from '../logic/CopyToClipboard';

interface Props {
  items: IItem[];
  isLoading: boolean;
}

class ItemContainer extends React.PureComponent<Props, object> {

  constructor(props: Props) {
    super(props);
  }

  transferSingle(url: object[]) {
    const id = url.join(';');
    if (isEmbedded) {
      document.location.href = `transfer://single/${id}`;
    } else {
      console.log('Transfer Single', id);
    }
  }

  transferAll(url: object[]) {
    const id = url.join(';');
    if (isEmbedded) {
      document.location.href = `transfer://all/${id}`;
    } else {
      console.log('Transfer All', id);
    }
  }

  getClipboardContent() {
    const colors = { Epic: 'DarkOrchid', Blue: 'RoyalBlue', Green: 'SeaGreen', Unknown: '', Yellow: 'Yellow' };
    const entries = this.props.items.map(item => {
      const name = item.name.replace('"', '');
      const entry = `[URL="http://www.grimtools.com/db/search?query=${name}"][COLOR="${colors[item.quality]}"]${item.name}[/COLOR][/URL]`;
      return entry;
    });

    return entries.join('\n');
  }

  render() {
    const items = this.props.items;

    if (this.props.isLoading) {
      return <Spinner/>;
    }
    else if (items.length > 0) {
      return (
        <div className="items">
          <span className="clipboard-link" onClick={() => copyToClipboard(this.getClipboardContent())}>
            {translate('app.copyToClipboard')}
          </span>

          {items.map((item) =>
            <Item
              item={item}
              key={'item-' + item.url.join(':')}
              transferAll={(url) => this.transferAll(url)}
              transferSingle={(url) => this.transferSingle(url)}
            />
          )}
          <ReactTooltip />

          <OnScrollLoader/>
        </div>
      );
    }
    else {
      return (
        <div className="no-items-found">
          {translate('items.label.noItemsFound')}
        </div>
      );
    }
  }

}

export function mapStateToProps(state: GlobalReducerState): Props {
  return {
    items: state.setItemReducer.items,
    isLoading: state.setItemReducer.isLoading
  };
}

export default connect<Props>(mapStateToProps)(ItemContainer);
