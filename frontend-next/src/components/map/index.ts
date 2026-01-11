// Only export AttendanceMap which uses dynamic import with SSR disabled
// Do NOT export MapComponent directly - it requires browser APIs (window/document)
export { default as AttendanceMap } from "./AttendanceMap";
