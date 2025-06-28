import React from 'react';

const FiltersBar = ({
  queueFilter,
  statusFilter,
  companyFilter,
  priorityFilter,
  queueOptions,
  statusOptions,
  companyOptions,
  priorityOptions,
  onQueueChange,
  onStatusChange,
  onCompanyChange,
  onPriorityChange,
}) => {
  const handleClearFilters = () => {
    onQueueChange('');
    onStatusChange('');
    onCompanyChange('');
    onPriorityChange('');
  };

  return (
    <div className="overflow-x-auto mb-6">
      <div className="grid grid-cols-5 gap-4 min-w-[900px] bg-gray-50 p-4 rounded border border-gray-200 shadow-sm">
        {/* Queue Filter */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Queue</label>
          <select
            className="w-full border border-gray-300 rounded-md px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={queueFilter}
            onChange={(e) => onQueueChange(e.target.value)}
          >
            <option value="">All</option>
            {queueOptions.map((q) => (
              <option key={q} value={q}>{q}</option>
            ))}
          </select>
        </div>

        {/* Status Filter */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Status</label>
          <select
            className="w-full border border-gray-300 rounded-md px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={statusFilter}
            onChange={(e) => onStatusChange(e.target.value)}
          >
            <option value="">All</option>
            {statusOptions.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>

        {/* Company Filter */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Company</label>
          <select
            className="w-full border border-gray-300 rounded-md px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={companyFilter}
            onChange={(e) => onCompanyChange(e.target.value)}
          >
            <option value="">All</option>
            {companyOptions.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
        </div>

        {/* Priority Filter */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Priority</label>
          <select
            className="w-full border border-gray-300 rounded-md px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={priorityFilter}
            onChange={(e) => onPriorityChange(e.target.value)}
          >
            <option value="">All</option>
            {priorityOptions.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>
        </div>

        {/* Clear Filters Button */}
        <div className="flex flex-col justify-end">
          <button
            onClick={handleClearFilters}
            className="bg-gray-200 hover:bg-gray-300 text-sm px-4 py-1 rounded-md transition ring-1 ring-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            Clear Filters
          </button>
        </div>
      </div>
    </div>
  );
};

export default FiltersBar;
