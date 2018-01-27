import * as React from 'react';
import { Tree } from './Tree';
import './CraftingContainer.css';
import SelectSearch from 'react-select-search';

const options = [
  {name: 'Swedish', value: 'sv'},
  {name: 'English', value: 'en'}
];
class CraftingContainer extends React.Component {
  render() {
    return (
      <div>
        <div>
          <SelectSearch options={options} value="sv" name="language" placeholder="Choose your language" />
          <SelectSearch options={options} value="sv" name="language" placeholder="Choose your language" />
          <SelectSearch options={options} value="sv" name="language" placeholder="Choose your language" />

        </div>

        <Tree />
      </div>
    );
  }
}

export default CraftingContainer;
