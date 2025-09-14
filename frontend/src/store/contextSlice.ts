import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface ContextState {
  selectedFranchiseId: string | null;
  selectedFranchiseName: string | null;
  selectedSeason: number | null;
  breadcrumb: Array<{
    label: string;
    path: string;
  }>;
  viewMode: 'global' | 'franchise'; // global view or franchise-specific view
}

const initialState: ContextState = {
  selectedFranchiseId: null,
  selectedFranchiseName: null,
  selectedSeason: null,
  breadcrumb: [],
  viewMode: 'global',
};

const contextSlice = createSlice({
  name: 'context',
  initialState,
  reducers: {
    setSelectedFranchise: (state, action: PayloadAction<{ id: string; name: string; preserveSeasonContext?: boolean }>) => {
      state.selectedFranchiseId = action.payload.id;
      state.selectedFranchiseName = action.payload.name;
      state.viewMode = 'franchise';
      // Clear season context unless explicitly preserving it
      if (!action.payload.preserveSeasonContext) {
        state.selectedSeason = null;
      }
    },
    clearSelectedFranchise: (state) => {
      state.selectedFranchiseId = null;
      state.selectedFranchiseName = null;
      state.viewMode = 'global';
    },
    setSelectedSeason: (state, action: PayloadAction<number | null>) => {
      state.selectedSeason = action.payload;
    },
    clearSelectedSeason: (state) => {
      state.selectedSeason = null;
    },
    setBreadcrumb: (state, action: PayloadAction<Array<{ label: string; path: string }>>) => {
      state.breadcrumb = action.payload;
    },
    addBreadcrumb: (state, action: PayloadAction<{ label: string; path: string }>) => {
      state.breadcrumb.push(action.payload);
    },
    clearBreadcrumb: (state) => {
      state.breadcrumb = [];
    },
    setViewMode: (state, action: PayloadAction<'global' | 'franchise'>) => {
      state.viewMode = action.payload;
    },
  },
});

export const {
  setSelectedFranchise,
  clearSelectedFranchise,
  setSelectedSeason,
  clearSelectedSeason,
  setBreadcrumb,
  addBreadcrumb,
  clearBreadcrumb,
  setViewMode,
} = contextSlice.actions;

export default contextSlice.reducer;