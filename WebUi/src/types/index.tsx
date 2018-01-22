import IItem from '../interfaces/IItem';

export interface GlobalState {
    clickCounter: number;
    items: IItem[];
    isLoading: boolean;
}
