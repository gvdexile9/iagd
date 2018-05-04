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

interface Props {
  items: IItem[];
  isLoading: boolean;
}

class ItemContainer extends React.Component<Props, object> {

  constructor(props: Props) {
    super(props);
    this.props = props;
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

  render() {
    const items = this.props.items;

    if (this.props.isLoading) {
      return <Spinner />;
    }
    else if (items.length > 0) {
      return (
        <div className="items">

          {items.map((item) =>
            <Item
              item={item}
              key={'item-' + item.url.join(':')}
              transferAll={(url) => this.transferAll(url)}
              transferSingle={(url) => this.transferSingle(url)}
            />
          )}

          <ReactTooltip id="you-can-craft-this-item-tooltip"><span>You can craft this item</span>
          </ReactTooltip>

          <OnScrollLoader />
        </div>
      );
    }
    else {
      return (
        <div className="no-items-found">
          No items found
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
