export default function ColorTestPage() {
  return (
    <div className="p-10 space-y-6">
      <div className="bg-yellow-300 text-black border-4 border-red-600 p-4 rounded text-center">
        ✅ Tailwind Block (bg-yellow-300, border-red-600)
      </div>

      <div className="bg-emerald-400 text-white p-4 rounded text-center">
        ✅ Tailwind Block (bg-emerald-400, text-white)
      </div>

      <div className="bg-gray-50 text-gray-700 p-4 rounded text-center border border-gray-300">
        ✅ Tailwind Block (bg-gray-50)
      </div>

      <div style={{ backgroundColor: 'yellow', border: '2px solid red', padding: '1rem' }}>
        ✅ Raw Inline Styles — If this is yellow + red, browser styles work
      </div>
    </div>
  );
}
