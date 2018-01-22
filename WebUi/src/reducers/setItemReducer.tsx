import { AnyAction } from '../actions';
import {ACTION_SET_ITEMS, ACTION_ADD_ITEMS, SET_MOCK_DATA, SET_LOADING_STATUS} from '../constants';
import { GlobalState } from '../types';


export function setItemReducer(state: GlobalState, action: AnyAction): GlobalState {
  if (action.type === ACTION_SET_ITEMS) {
    console.log(`Setting ${action.items.length} items to the item view`);
    return {...state, items: action.items, isLoading: false};
  }

  else if (action.type === ACTION_ADD_ITEMS) {
    console.log(`Adding ${action.items.length} items to the item view`);
    return {...state, items: action.items, isLoading: false};
  }

  else if (action.type === SET_MOCK_DATA) {
    const items = JSON.parse(action.items);
    console.log('Setting mock items');
    return {...state, items: items, isLoading: false};
  }

  else if (action.type === SET_LOADING_STATUS) {
    console.log('Update state for loading status to', action.status);
    return {...state, isLoading: action.status};
  }

  return state;
}
