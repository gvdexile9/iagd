import * as React from 'react';
import 'react-select/dist/react-select.css';
import ItemStat from './ItemStat';
import IItem from '../interfaces/IItem';
import Skill from './Skill';
import * as ReactTooltip from 'react-tooltip';
import { isEmbedded } from '../constants/index';
import * as Guid from 'guid';

const buddyIcon = require('./img/buddy.png');
const recipeIcon = require('./img/recipe.png');
const cloudErrIcon = require('./img/cloud-err.png');
const cloudOkIcon = require('./img/cloud-ok.png');

//import buddyIcon from './img/buddy.png';

interface Props {
  item: IItem;
  transferSingle: (x: any) => void;
  transferAll: (x: any) => void;
}

class Item extends React.Component<Props, object> {

  openItemSite() {
    if (isEmbedded) {
      document.location.href = `http://www.grimtools.com/db/search?query=${this.props.item.name}`;
    } else {
      window.open(`http://www.grimtools.com/db/search?query=${this.props.item.name}`);
    }
  }

  renderBuddyItem(item: IItem) {
    //const tooltipId = 'tooltip-buddy-' + item.url.join(':');
    if (item.type === 2) {
      if (item.buddies.length === 1) {
        return (
          <div className="buddy-item-mix">
            &nbsp;&laquo;{item.buddies[0]}&raquo; also has this item.
          </div>
        );
      } else {
        return (
          <div></div>
          /*<div className="buddy-item-mix" data-bind="attr: {title: buddies.join()}">
            <a data-tip="true" data-for={tooltipId}>
              &laquo;{item.buddies.length}&raquo; of your Buddies also have this item.
            </a>

            <ReactTooltip id={tooltipId}><span>{item.buddies.join(', ')}</span>
            </ReactTooltip>
          </div>*/
        );
      }
    } else {
      if (item.buddies.length === 1) {
        return (
          <div className="buddy-item">
            &nbsp;&laquo;{item.buddies[0]}&raquo; has this item.
          </div>
        );
      } else {
        return (
          <div className="buddy-item">
            &nbsp;&laquo;{item.buddies.length}&raquo; of your Buddies have this item.
          </div>
        );
      }
    }
  }

  translateQualityToClass(quality: string): string {
    return `item-quality-${quality.toLowerCase()}`;
  }

  renderCornerContainer(item: IItem) {
    const recipeTooltip = Guid.raw();
    return (
      <div className="recipe-item-corner">
        {item.type === 2 && item.hasCloudBackup &&
        <img
          className="cursor-help"
          data-bind="attr: {title: $root.iatag_html_cloud_ok}"
          src={cloudOkIcon}
          title="TODO: Cloud OK"
        />
        }

        {item.type === 2 && !item.hasCloudBackup &&
        <img
          className="cursor-help"
          data-bind="attr: {title: $root.iatag_html_cloud_err}"
          src={cloudErrIcon}
          title="TODO: Cloud err"
        />
        }
        {item.type !== 1 &&
        <span>
          {item.buddies.length === 1 &&
          <img
            className="cursor-help"
            data-bind="attr: {title: buddies[0] + ' ' + $root.iatag_html_items_buddy_alsohasthisitem1}"
            src={buddyIcon}
            title="TODO: 1 buddy has this item"
          />
          }

          {item.buddies.length > 1 &&
          <img
            className="cursor-help"
            data-bind="attr: {title: $root.iatag_html_items_buddy_alsohasthisitem2 + '\n' + buddies.join('\n')}"
            src={buddyIcon}
            title="TODO: 2+ buddies has this item"
          />
          }
        </span>
        }
        {item.hasRecipe && item.type !== 0 &&
        <span data-tip="true" data-for={recipeTooltip}>
          <img
            className="cursor-help"
            data-bind="attr: {title: $root.iatag_html_items_youcancraftthisitem}, click: function(item) { jumpToCraft(item.baseRecord); }"
            src={recipeIcon}
          />
          <ReactTooltip id={recipeTooltip}><span>TODO: You can craft this item</span>
          </ReactTooltip>
          </span>
        }
      </div>
    );
  }

  render() {
    const item = this.props.item;
    const icon = item.name.length > 0 ? item.icon : 'weapon1h_focus02a.tex.png';
    const name = item.name.length > 0 ? item.name : 'Unknown';

    const headerStats = item.headerStats.map((stat) =>
      <ItemStat label={stat.label} extras={stat.extras} key={'stat-head-' + item.url.join(':') + stat.label}/>
    );

    const bodyStats = item.bodyStats.map((stat) =>
      <ItemStat label={stat.label} extras={stat.extras} key={'stat-body-' + item.url.join(':') + stat.label}/>
    );

    const petStats = item.petStats.map((stat) =>
      <ItemStat label={stat.label} extras={stat.extras} key={'stat-pets-' + item.url.join(':') + stat.label}/>
    );

    const itemLogoSlotTooltip = Guid.raw();
    return (
      <div className="item">
        <span>
          <img src={icon} className="item-icon" data-tip="true" data-for={itemLogoSlotTooltip}/>
          <ReactTooltip id={itemLogoSlotTooltip}><span>{item.slot}</span>
          </ReactTooltip>
        </span>
        <div className="text">
          <h3 className="item-name-header">
            <span>
              <a onClick={() => this.openItemSite()} className={this.translateQualityToClass(item.quality)}>{name}</a>
            </span>
            {item.greenRarity === 3 ? <span className="cursor-help">(+2)</span> : ''/* TODO: Tooltips */}
            {item.greenRarity === 2 ? <span className="cursor-help">(+1)</span> : ''/* TODO: Tooltips */}
          </h3>
          {item.socket && item.socket.length > 0 &&
          <span className="item-socket-label">{item.socket}</span>
          }

          <ul>
            {headerStats}
          </ul>

          <ul className="bodystats">
            {bodyStats}
          </ul>

          {petStats.length > 0 ? (
            <div>
              <div className="pet-header">Bonus to All Pets:</div>
              {petStats}
            </div>
          ) : ''
          }

          {item.skill ? <Skill skill={item.skill} keyPrefix={item.url.join(':')}/> : ''}

        </div>
        {item.buddies.length > 0 ? this.renderBuddyItem(item) : ''}

        {item.hasRecipe && item.type !== 0 ?
          <span>
            <a data-tip="true" data-for="you-can-craft-this-item-tooltip">
              <div className="recipe-item-corner">
                <img className="cursor-help" src={recipeIcon}/>
              </div>
            </a>
          </span>
          : ''
        }

        {this.renderCornerContainer(item)}

        {item.hasRecipe && item.type === 0 ?
          <div className="recipe-item">
            <img src={recipeIcon}/>
            <span className="craft-link" data-bind="click: function(item) { jumpToCraft(item.baseRecord); }">
              &nbsp;<span>You can craft this item</span>
            </span>
          </div>
          : ''
        }

        <div className="level">
          <p>Level Requirement: {item.level > 1 ? item.level : 'Any'}</p>
        </div>

        {item.numItems > 1 && item.type === 2 ?
          <div className="link-container-all">
            <a onClick={() => this.props.transferAll(item.url)}>Transfer All ({item.numItems})</a>
          </div>
          : ''
        }

        {item.type === 2 ?
          <div className="link-container">
            <a onClick={() => this.props.transferSingle(item.url)}>Transfer</a>
          </div>
          : ''
        }
      </div>
    );
  }
}

export default Item;
