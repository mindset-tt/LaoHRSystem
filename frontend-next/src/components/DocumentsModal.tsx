import { useState, useEffect } from "react";
import { X, Upload, FileText, Trash2, Download } from "lucide-react";
import { Employee, EmployeeDocument, documentsAPI } from "@/lib/api";

interface DocumentsModalProps {
    employee: Employee | null;
    isOpen: boolean;
    onClose: () => void;
}

export default function DocumentsModal({ employee, isOpen, onClose }: DocumentsModalProps) {
    const [documents, setDocuments] = useState<EmployeeDocument[]>([]);
    const [loading, setLoading] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [docType, setDocType] = useState("Contract");
    const [file, setFile] = useState<File | null>(null);

    useEffect(() => {
        if (isOpen && employee) {
            loadDocuments();
        }
    }, [isOpen, employee]);

    const loadDocuments = async () => {
        if (!employee) return;
        setLoading(true);
        try {
            const docs = await documentsAPI.getByEmployee(employee.id);
            setDocuments(docs);
        } catch (error) {
            console.error("Failed to load documents", error);
        } finally {
            setLoading(false);
        }
    };

    const handleUpload = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!employee || !file) return;

        setUploading(true);
        try {
            await documentsAPI.upload(employee.id, docType, file);
            setFile(null);
            // Reset file input
            const fileInput = document.getElementById('file-upload') as HTMLInputElement;
            if (fileInput) fileInput.value = '';

            await loadDocuments();
        } catch (error) {
            console.error("Failed to upload document", error);
            alert("Failed to upload document");
        } finally {
            setUploading(false);
        }
    };

    const handleDelete = async (id: number) => {
        if (!confirm("Are you sure you want to delete this document?")) return;
        try {
            await documentsAPI.delete(id);
            setDocuments(documents.filter(d => d.documentId !== id));
        } catch (error) {
            console.error("Failed to delete document", error);
        }
    };

    if (!isOpen || !employee) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <div className="bg-slate-900 border border-slate-800 rounded-xl w-full max-w-2xl max-h-[90vh] flex flex-col">
                <div className="p-6 border-b border-slate-800 flex justify-between items-center bg-slate-900 sticky top-0 z-10">
                    <div>
                        <h2 className="text-xl font-semibold text-slate-100">Documents</h2>
                        <p className="text-sm text-slate-500">Manage files for {employee.laoName}</p>
                    </div>
                    <button onClick={onClose} className="text-slate-500 hover:text-slate-300 transition-colors">
                        <X className="w-6 h-6" />
                    </button>
                </div>

                <div className="p-6 overflow-y-auto flex-1">
                    {/* Upload Section */}
                    <form onSubmit={handleUpload} className="mb-8 p-4 bg-slate-800/50 border border-slate-700 rounded-lg">
                        <h3 className="text-sm font-medium text-slate-300 mb-4 flex items-center gap-2">
                            <Upload className="w-4 h-4" />
                            Upload New Document
                        </h3>
                        <div className="grid grid-cols-1 sm:grid-cols-10 gap-4 items-end">
                            <div className="sm:col-span-3">
                                <label className="block text-xs text-slate-500 mb-1">Type</label>
                                <select
                                    value={docType}
                                    onChange={(e) => setDocType(e.target.value)}
                                    className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-200 text-sm focus:outline-none focus:border-indigo-500"
                                >
                                    <option value="Contract">Contract</option>
                                    <option value="ID_Card">ID Card / Passport</option>
                                    <option value="Resume">Resume / CV</option>
                                    <option value="Certificate">Certificate</option>
                                    <option value="Other">Other</option>
                                </select>
                            </div>
                            <div className="sm:col-span-5">
                                <label className="block text-xs text-slate-500 mb-1">File</label>
                                <input
                                    type="file"
                                    id="file-upload"
                                    onChange={(e) => setFile(e.target.files?.[0] || null)}
                                    className="w-full text-sm text-slate-400 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-indigo-600 file:text-white hover:file:bg-indigo-500"
                                    accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                                />
                            </div>
                            <div className="sm:col-span-2">
                                <button
                                    type="submit"
                                    disabled={!file || uploading}
                                    className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-medium rounded-lg transition-colors"
                                >
                                    {uploading ? "Uploading..." : "Upload"}
                                </button>
                            </div>
                        </div>
                    </form>

                    {/* Documents List */}
                    <div className="space-y-3">
                        {loading ? (
                            <div className="text-center py-8 text-slate-500">Loading documents...</div>
                        ) : documents.length === 0 ? (
                            <div className="text-center py-8 text-slate-500 border-2 border-dashed border-slate-800 rounded-lg">
                                <FileText className="w-8 h-8 mx-auto mb-2 opacity-50" />
                                <p>No documents uploaded yet</p>
                            </div>
                        ) : (
                            documents.map((doc) => (
                                <div key={doc.documentId} className="flex items-center justify-between p-4 bg-slate-800 border border-slate-700 rounded-lg hover:border-slate-600 transition-colors">
                                    <div className="flex items-center gap-4">
                                        <div className="w-10 h-10 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-400">
                                            <FileText className="w-5 h-5" />
                                        </div>
                                        <div>
                                            <h4 className="font-medium text-slate-200">{doc.documentType.replace('_', ' ')}</h4>
                                            <p className="text-xs text-slate-500">{doc.fileName}</p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <span className="text-xs text-slate-500 mr-2">
                                            {new Date(doc.uploadedAt + 'Z').toLocaleDateString()}
                                        </span>
                                        <a
                                            href={`http://localhost:5000${doc.filePath}`}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="p-2 text-slate-400 hover:text-white hover:bg-slate-700 rounded-lg transition-colors"
                                            title="Download/View"
                                        >
                                            <Download className="w-4 h-4" />
                                        </a>
                                        <button
                                            onClick={() => handleDelete(doc.documentId)}
                                            className="p-2 text-slate-400 hover:text-red-400 hover:bg-slate-700 rounded-lg transition-colors"
                                            title="Delete"
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </button>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
