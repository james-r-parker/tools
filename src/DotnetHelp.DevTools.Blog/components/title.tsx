import Tags from "@/components/tags";
import FormattedDate from "./date";

export default async function Title({ title, overview, date, author, tags }: { title: string, overview: string, date?: string, author?: string, tags?: string[] }) {
    return (
        <header className="mb-5 md:flex md:items-start">
            <div className="flex-auto max-w-4xl">
                <h1 className="mb-4 text-sm leading-6 font-semibold text-sky-500 dark:text-sky-400">{title}</h1>
                <h2 className="mb-2 text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight dark:text-slate-200">{overview}</h2>
                {author &&
                    <h3 className="mb-2 text-xl sm:text-lg text-slate-600 dark:text-slate-400">{author}</h3>
                }
                {date &&
                    <p className="mb-2 text-sm sm:text-xs text-slate-600 dark:text-slate-400"><FormattedDate date={date} /></p>
                }
            </div>
            {tags &&
                <div>
                    <Tags tags={tags} />
                </div>
            }
        </header>
    );
}